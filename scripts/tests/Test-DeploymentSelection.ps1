<#
.SYNOPSIS
    Verify preprovision selection prompts without Azure calls or interactive input.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$hook = Join-Path $repoRoot 'infra\scripts\preprovision.ps1'
$selectionNames = @(
    'DEPLOY_BATCH', 'DEPLOY_ACI', 'DEPLOY_CONTAINERAPP', 'DEPLOY_AKS',
    'DEPLOY_SQLSERVER', 'DEPLOY_POSTGRESQL', 'DEPLOY_MYSQL'
)
$script:state = $null
$checks = 0

function az {
    $global:LASTEXITCODE = 0
    switch ($args -join ' ') {
        'provider register --namespace Microsoft.Relay --wait' { return }
        'ad signed-in-user show --query id -o tsv' { return 'offline-user-id' }
        'account show --query user.name -o tsv' { return 'offline-user@example.invalid' }
        default { throw "Unexpected Azure command: $args" }
    }
}

function azd {
    $global:LASTEXITCODE = 0
    $name = $args[2]
    switch ($args[0..1] -join ' ') {
        'env get-value' {
            if ($name -eq $state.FailedRead -or -not $state.Values.ContainsKey($name)) {
                $global:LASTEXITCODE = 1
            }
            return $state.Values[$name]
        }
        'env set' {
            $state.Values[$name] = $args[3]
            if ($name -in $selectionNames) { $state.Saved.Add($name) }
        }
        default { throw "Unexpected azd command: $args" }
    }
}

function Invoke-WebRequest {
    param($Uri, [switch] $UseBasicParsing)
    if ($Uri -ne 'https://api.ipify.org') { throw "Unexpected request: $Uri" }
    return @{ Content = '192.0.2.1' }
}

function Read-Host {
    param($Prompt)
    $state.Prompts.Add($Prompt)
    return '1'
}

function Write-Host {}

function Test-SelectionScenario([hashtable] $Values, [bool] $ExpectPrompt, [string] $FailedRead = '') {
    $script:state = @{
        Values = $Values.Clone()
        FailedRead = $FailedRead
        Prompts = [Collections.Generic.List[string]]::new()
        Saved = [Collections.Generic.List[string]]::new()
    }
    $state.Values.MYSQL_AUTH_MODE = 'ManagedIdentity'
    $state.Values.PG_ADMIN_PASSWORD = 'offline-placeholder'
    $state.Values.MYSQL_ADMIN_PASSWORD = 'offline-placeholder'
    & $hook

    $expectedPrompts = if ($ExpectPrompt) { 2 } else { 0 }
    if ($state.Prompts.Count -ne $expectedPrompts) {
        throw "Expected $expectedPrompts prompts; received $($state.Prompts.Count)."
    }
    if ($ExpectPrompt) {
        if ($state.Saved.Count -ne 7 -or @(Compare-Object $selectionNames $state.Saved).Count -ne 0) {
            throw 'Prompting must save all seven selections.'
        }
        foreach ($name in $selectionNames) {
            $expected = if ($name -in @('DEPLOY_BATCH', 'DEPLOY_SQLSERVER')) { 'true' } else { 'false' }
            if ($state.Values[$name] -ne $expected) { throw "Prompt result was not saved for $name." }
        }
    } else {
        if ($state.Saved.Count -ne 0) { throw 'Complete selections must not be overwritten.' }
        foreach ($name in $selectionNames) {
            if ($state.Values[$name] -ne $Values[$name]) { throw "Saved selection changed for $name." }
        }
    }
}

foreach ($savedValue in @('true', 'false')) {
    $complete = @{}
    foreach ($name in $selectionNames) { $complete[$name] = $savedValue }
    Test-SelectionScenario $complete $false
    $checks++
}
Test-SelectionScenario @{} $true
$checks++

foreach ($name in $selectionNames) {
    foreach ($missingValue in @($null, '', '   ', 'ERROR: missing value')) {
        $incomplete = $complete.Clone()
        if ($null -eq $missingValue) { $incomplete.Remove($name) }
        else { $incomplete[$name] = $missingValue }
        Test-SelectionScenario $incomplete $true
        $checks++
    }
    Test-SelectionScenario $complete $true -FailedRead $name
    $checks++
}

[Console]::WriteLine("Deployment selection checks passed: $checks scenarios.")
