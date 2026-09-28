<#
.SYNOPSIS
    Checks database startup gates and runner ordering offline, without Azure calls.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $PSScriptRoot 'aci_test_helpers.ps1')
$fixture = [IO.Directory]::CreateTempSubdirectory('SbmDatabaseStartup-').FullName
$previousCI = $env:CI
$script:state = $null
$checks = 0

function Assert-Startup([bool] $condition, [string] $message) {
    if (-not $condition) { throw $message }
}

function Reset-StartupState([string] $platform, [hashtable] $servers) {
    $script:state = @{
        Platform = $platform
        Servers = $servers
        Current = @{}
        Starts = [Collections.Generic.List[string]]::new()
        Events = [Collections.Generic.List[string]]::new()
        Now = [datetime]'2026-01-01T00:00:00Z'
        ShowError = $false
        StartError = $false
        Deployed = $true
        Compute = $true
        Runs = 0
    }
}

function Get-Date {
    param([string] $Format)
    if ($Format) { return $state.Now.ToString($Format) }
    return $state.Now
}

function Start-Sleep {
    param([int] $Seconds)
    $state.Now = $state.Now.AddSeconds($Seconds)
}

function Write-Host {
    param($Object, $ForegroundColor)
    $state.Events.Add([string]$Object)
}

function Get-Command {
    param($Name, $ErrorAction)
    if ($Name -eq 'copilot') { return $null }
    throw "Unexpected command lookup: $Name"
}

function azd {
    if (($args -join ' ') -ne 'env get-values -e offline') { throw "Unexpected azd call: $args" }
    $global:LASTEXITCODE = 0
    "DEPLOY_MYSQL=`"$($state.Deployed.ToString().ToLowerInvariant())`""
    "DEPLOY_POSTGRESQL=`"$($state.Deployed.ToString().ToLowerInvariant())`""
    "DEPLOY_ACI=`"$($state.Compute.ToString().ToLowerInvariant())`""
    'DEPLOY_BATCH="false"'
    'DEPLOY_CONTAINERAPP="false"'
    'DEPLOY_AKS="false"'
    'MYSQL_AUTH_MODE="ManagedIdentity"'
}

function az {
    Assert-Startup ($args[0] -eq $state.Platform -and $args[1] -eq 'flexible-server') "Unexpected Azure command: $args"
    $name = $args[[Array]::IndexOf($args, '--name') + 1]
    $group = $args[[Array]::IndexOf($args, '--resource-group') + 1]
    Assert-Startup ($group -eq 'rg-offline') "Unexpected resource group: $group"
    Assert-Startup ($state.Servers.ContainsKey($name)) "Unexpected server: $name"
    $global:LASTEXITCODE = 0
    switch ($args[2]) {
        'show' {
            Assert-Startup ($args -contains '--query' -and $args -contains 'state') 'Must query server state.'
            if ($state.ShowError) {
                $global:LASTEXITCODE = 1
                return 'mock read denied'
            }
            $sequence = @($state.Servers[$name])
            $current = $sequence[0]
            if ($sequence.Count -gt 1) { $state.Servers[$name] = $sequence[1..($sequence.Count - 1)] }
            $state.Current[$name] = $current
            $state.Events.Add("show:$name=$current")
            return $current
        }
        'start' {
            Assert-Startup ($args -contains '--no-wait') 'Start should return before polling.'
            Assert-Startup ($state.Current[$name] -eq 'Stopped') 'Only stopped servers may be started.'
            $state.Starts.Add($name)
            if ($state.StartError) {
                $global:LASTEXITCODE = 1
                return 'mock start denied'
            }
        }
        default { throw "Unexpected Azure operation: $args" }
    }
}

function Invoke-StartupCheck([scriptblock] $action, [string] $expectedError = '') {
    $failure = ''
    try { & $action } catch { $failure = $_.Exception.Message }
    if ($expectedError) {
        Assert-Startup ($failure.Contains($expectedError)) "Expected '$expectedError', got '$failure'."
        Assert-Startup ($state.Runs -eq 0) 'Tests must not launch after a failed readiness check.'
    } else {
        Assert-Startup ($failure -eq '') "Unexpected failure: $failure"
    }
}

try {
    $env:CI = 'true'
    $testScripts = Join-Path $fixture 'scripts\tests'
    New-Item -ItemType Directory -Path $testScripts, (Join-Path $fixture 'infra') | Out-Null
    Copy-Item (Join-Path $repoRoot 'infra\resourcetypes.json') (Join-Path $fixture 'infra')
    Copy-Item (Join-Path $repoRoot 'scripts\prefix_resource_names.ps1') (Join-Path $fixture 'scripts')
    foreach ($name in @('aci_test_helpers.ps1', 'run_all_mysql_external_tests_in_aci.ps1', 'run_all_postgres_external_tests_in_aci.ps1')) {
        Copy-Item (Join-Path $PSScriptRoot $name) $testScripts
    }
    @'
param($envName, $customName, $testFilter, $timeoutMinutes, $timestamp, [switch] $buildImage)
Assert-Startup ($state.Current.Count -eq 2) 'Both servers must be checked before running tests.'
foreach ($current in $state.Current.Values) {
    Assert-Startup ($current -eq 'Ready') 'Both servers must be Ready before running tests.'
}
Assert-Startup ($envName -eq 'offline' -and $testFilter -like '*AzureTest.AciTests') 'Runner selection changed.'
Assert-Startup ($timeoutMinutes -eq 300) 'Test timeout changed.'
if ($state.Platform -eq 'mysql') {
    Assert-Startup ($buildImage -and $customName -eq 'mysql') 'MySQL runner options changed.'
} else {
    Assert-Startup ($customName -eq 'pg') 'PostgreSQL runner options changed.'
}
Assert-Startup ($state.Events[-1] -like '*Starting the * external test run in ACI*') 'Missing test-launch notification.'
$state.Events.Add('tests')
$state.Runs++
$global:LASTEXITCODE = 0
'@ | Set-Content (Join-Path $testScripts 'run_filtered_external_tests_in_aci.ps1')

    foreach ($platform in @('mysql', 'postgres')) {
        foreach ($sequence in @(
            @('Ready'), @('Stopped', 'Starting', 'Ready'), @('Starting', 'Ready'),
            @('Stopping', 'Stopped', 'Starting', 'Ready'), @('Updating', 'Ready'),
            @('Stopped', 'Stopped', 'Starting', 'Ready')
        )) {
            Reset-StartupState $platform @{ 'server-a' = $sequence; 'server-b' = @('Ready') }
            Invoke-StartupCheck {
                Start-AzureDatabaseServersForTests -platform $platform -resourceGroupName rg-offline -serverNames server-a,server-b -timeoutSeconds 5 -pollIntervalSeconds 1
            }
            $expectedStarts = if ($sequence -contains 'Stopped') { 1 } else { 0 }
            Assert-Startup ($state.Starts.Count -eq $expectedStarts) 'Incorrect number of start requests.'
            Assert-Startup ($state.Current['server-b'] -eq 'Ready') 'Second server was not checked.'
            if ($expectedStarts) {
                Assert-Startup (@($state.Events | Where-Object { $_ -like 'Starting stopped *' }).Count -eq 1) 'Missing server-start notification.'
            }
            $checks++
        }

        foreach ($case in @('read-error', 'start-error', 'blank', 'Disabled', 'Failed', 'timeout')) {
            $initial = switch ($case) {
                'blank' { '' }
                'timeout' { 'Starting' }
                'read-error' { 'Ready' }
                'start-error' { 'Stopped' }
                default { $case }
            }
            Reset-StartupState $platform @{ 'server-a' = @($initial) }
            $state.ShowError = $case -eq 'read-error'
            $state.StartError = $case -eq 'start-error'
            $expected = switch ($case) {
                'read-error' { 'mock read denied' }
                'start-error' { 'mock start denied' }
                'timeout' { 'Timed out waiting' }
                default { 'Unexpected state' }
            }
            Invoke-StartupCheck {
                Start-AzureDatabaseServersForTests -platform $platform -resourceGroupName rg-offline -serverNames server-a -timeoutSeconds 2 -pollIntervalSeconds 1
            } $expected
            $checks++
        }

        $prefix = if ($platform -eq 'mysql') { 'mysql' } else { 'psql' }
        $wrapper = Join-Path $testScripts "run_all_${platform}_external_tests_in_aci.ps1"
        foreach ($case in @('ready', 'stopped', 'not-deployed', 'no-compute', 'unavailable-group', 'failed', 'second-failed', 'start-failed', 'timeout')) {
            Reset-StartupState $platform @{
                "$prefix-offline-a" = @('Ready')
                "$prefix-offline-b" = @('Ready')
            }
            if ($case -eq 'stopped') {
                $state.Servers["$prefix-offline-a"] = @('Stopped', 'Starting', 'Ready')
                $state.Servers["$prefix-offline-b"] = @('Stopped', 'Ready')
            }
            $state.Deployed = $case -ne 'not-deployed'
            $state.Compute = $case -ne 'no-compute'
            $state.ShowError = $case -eq 'failed'
            if ($case -eq 'second-failed') { $state.Servers["$prefix-offline-b"] = @('Failed') }
            if ($case -eq 'start-failed') {
                $state.Servers["$prefix-offline-a"] = @('Stopped')
                $state.StartError = $true
            }
            if ($case -eq 'timeout') { $state.Servers["$prefix-offline-a"] = @('Starting') }
            $options = @{ envName = 'offline'; testGroups = @('aci') }
            if ($platform -eq 'mysql') { $options.buildImage = $true }
            if ($case -eq 'unavailable-group') { $options.testGroups = @('batch') }
            $expected = switch ($case) {
                'failed' { 'mock read denied' }
                'second-failed' { 'Unexpected state' }
                'start-failed' { 'mock start denied' }
                'timeout' { 'Timed out waiting' }
                default { '' }
            }
            Invoke-StartupCheck { & $wrapper @options } $expected
            $shouldRun = $case -in @('ready', 'stopped')
            Assert-Startup ($state.Runs -eq [int]$shouldRun) "Unexpected test launches for $case."
            if ($case -in @('not-deployed', 'no-compute', 'unavailable-group', 'failed')) {
                Assert-Startup ($state.Current.Count -eq 0 -and $state.Starts.Count -eq 0) 'Skipped/failed runs must not start servers.'
            }
            $checks++
        }
    }
} finally {
    $env:CI = $previousCI
    Remove-Item -LiteralPath $fixture -Recurse -Force
}

[Console]::WriteLine("Database startup checks passed: $checks scenarios.")
