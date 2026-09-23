<#
.SYNOPSIS
    Offline two-environment regression for generated configuration, image staging and MSBuild selection.
.DESCRIPTION
    Runs actual producers with mocked az/azd/sbm, stages real files with robocopy,
    and exercises the shared MSBuild targets. No Azure calls, database connections or Docker daemon.
.PARAMETER BuildAzureProjects
    Also build the three real Azure test projects with synthetic configuration, then
    discover (not execute) their tests with --no-build after switching environments.
    Requires previously restored dependencies.
#>
[CmdletBinding()]
param([switch] $BuildAzureProjects)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$sourceRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$fixture = Join-Path ([IO.Path]::GetTempPath()) "sbm-env-config-$([guid]::NewGuid())"
$savedProjectPath = $env:AZD_PROJECT_PATH
$savedEnvironment = $env:AZURE_ENV_NAME
$state = @{ Checks = 0; Builds = 0; BuildExitCode = 0; SourceDirty = $false; Contexts = [Collections.Generic.List[string]]::new() }

function Assert-Config([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
    $state.Checks++
}
function Assert-ConfigFailure([scriptblock] $Action, [string] $Pattern) {
    $message = ''
    try { & $Action | Out-Null } catch { $message = $_.Exception.Message }
    Assert-Config ($message -match $Pattern) "Expected '$Pattern', received '$message'."
}
function Get-Argument($Arguments, [string] $Name) {
    $index = [Array]::IndexOf($Arguments, $Name)
    if ($index -lt 0) { throw "Missing argument '$Name'." }
    return $Arguments[$index + 1]
}
function azd {
    $global:LASTEXITCODE = 0
    $selected = Get-Argument $args '-e'
    Assert-Config ($selected -in @('alpha', 'bravo')) 'azd reads must select the requested environment explicitly.'
    if ($args[2] -eq 'RELAY_PROXY_ENDPOINT') {
        $state.RelayEnvironment = $selected
        $global:LASTEXITCODE = 1
        return ''
    }
    return "$selected-$($args[2])"
}
function git {
    $global:LASTEXITCODE = 0
    Assert-Config ((Get-Argument $args '-C') -eq $fixture) 'Source provenance must use the staged repository.'
    if ($args[2] -eq 'rev-parse') { return ('a' * 40) }
    if ($args[2] -eq 'status') {
        if ($state.SourceDirty) { return ' M src/example.cs' }
        return
    }
    throw "Unexpected Git command: $args"
}
function az {
    $global:LASTEXITCODE = 0
    $command = $args -join ' '
    switch -Regex ($command) {
        '^account show ' { return '11111111-2222-3333-4444-555555555555' }
        '^identity show ' { return '{"clientId":"11111111-2222-3333-4444-555555555555","principalId":"22222222-2222-3333-4444-555555555555","resourceGroup":"rg-offline","id":"/subscriptions/offline/resourceGroups/rg-offline/providers/Microsoft.ManagedIdentity/userAssignedIdentities/offline"}' }
        '^eventhubs eventhub list ' { return 'test-eventhub' }
        '^batch account show ' { return 'test.batch.azure.com' }
        '^containerapp env show ' { return 'test-region' }
        '^acr show ' { return 'test.azurecr.io' }
        '^storage blob download-batch ' { $global:LASTEXITCODE = 1; return 'Forbidden by network rules' }
        '^sql server list ' {
            $group = Get-Argument $args '--resource-group'
            return (@{ name = "$group-sql"; fullyQualifiedDomainName = "$group.database.windows.net" } | ConvertTo-Json -Compress)
        }
        '^sql db list ' { return @('master', 'SqlBuildTest1', 'SqlBuildTest2') }
        '^(mysql|postgres) flexible-server show ' {
            $server = Get-Argument $args '--name'
            return (@{ fullyQualifiedDomainName = "$server.$($args[0]).database.azure.com" } | ConvertTo-Json -Compress)
        }
        '^mysql flexible-server db list ' { return @('mysql', 'sbm_mysql_test1', 'sbm_mysql_test2') }
        '^postgres flexible-server db list ' { return @('postgres', 'sbm_pg_test1', 'sbm_pg_test2') }
        '^acr build ' {
            $context = $args[-1]
            $selected = (Get-Argument $args '--build-arg').Replace('AZD_ENV_NAME=', '')
            $state.Contexts.Add($context)
            Assert-Config ((Get-Argument $args '--resource-group') -eq 'rg-explicit') 'Image builder lost explicit resource group.'
            Assert-Config ((Get-Argument $args '--image') -eq 'sqlbuildmanager-tests:isolated') 'Image tag was lost.'
            $revision = ('a' * 40) + $(if ($state.SourceDirty) { '-dirty' } else { '' })
            Assert-Config ($args -contains "SBM_BUILD_REVISION=$revision") 'Image source revision/dirty marker was lost.'
            Assert-Config ((Get-Argument $args '--query') -eq '{runId:runId,images:outputImages}') 'Image build output must include exact digest metadata.'
            $entries = @(Get-ChildItem -LiteralPath (Join-Path $context 'TestConfig'))
            Assert-Config ($entries.Count -eq 1 -and $entries[0].Name -eq $selected) 'Image context contains another environment or root files.'
            $files = @(Get-ChildItem -LiteralPath $entries[0].FullName -Recurse -File)
            Assert-Config ($files.Count -eq $state.ExpectedFiles) 'Image context has missing or extra configuration files.'
            Assert-Config (-not ($files.Name -match 'legacy|stale-result|bundle.zip')) 'Image context includes root secrets, results or build artifacts.'
            Assert-Config (-not (Test-Path (Join-Path $context 'bin'))) 'Build output was included in context.'
            $state.Builds++
            $global:LASTEXITCODE = $state.BuildExitCode
            return
        }
        default { throw "Unexpected Azure command: $command" }
    }
}
function Invoke-ConfigSbm {
    $arguments = @($args | ForEach-Object { $_ })
    $file = Get-Argument $arguments '--settingsfile'
    $key = Get-Argument $arguments '--settingsfilekey'
    Assert-Config ((Split-Path $file -Parent) -eq (Split-Path $key -Parent)) 'Settings and encryption key must share a directory.'
    Assert-Config (Test-Path -LiteralPath $key) 'Settings key was not created.'
    @{ identity = (Get-Argument $arguments '--identityname'); platform = (Get-Argument $arguments '--platform') } |
        ConvertTo-Json | Set-Content -LiteralPath $file
    $global:LASTEXITCODE = 0
}

try {
    $env:AZD_PROJECT_PATH = $fixture
    $env:AZURE_ENV_NAME = ''
    New-Item -ItemType Directory -Path (Join-Path $fixture 'scripts'), (Join-Path $fixture 'infra'), (Join-Path $fixture 'src') -Force | Out-Null
    foreach ($relative in @(
        'scripts\test_config_paths.ps1', 'scripts\key_file_names.ps1', 'scripts\prefix_resource_names.ps1',
        'scripts\create_all_settingsfiles_mi_only.ps1', 'scripts\create_all_settingsfiles_mysql_password.ps1',
        'scripts\aci\create_aci_settingsfile_mi_only.ps1', 'scripts\Batch\create_batch_settingsfiles_mi_only.ps1',
        'scripts\ContainerApp\create_containerapp_settingsfile_mi_only.ps1', 'scripts\kubernetes\create_aks_settingsfile_mi_only.ps1',
        'scripts\Database\create_database_override_files.ps1', 'scripts\Database\create_pg_database_override_files.ps1',
        'scripts\Database\create_mysql_database_override_files.ps1', 'scripts\ContainerRegistry\build_external_test_image.ps1',
        'infra\resourcetypes.json', 'src\AzureTestConfig.targets', 'src\Dockerfile.tests')) {
        $destination = Join-Path $fixture $relative
        New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $sourceRoot $relative) -Destination $destination
    }
    . (Join-Path $fixture 'scripts\test_config_paths.ps1')
    $configRoot = Join-Path $fixture 'src\TestConfig'
    New-Item -ItemType Directory -Path $configRoot | Out-Null
    'legacy-root-sentinel' | Set-Content (Join-Path $configRoot 'legacy.txt')
    New-Item -ItemType Directory -Path (Join-Path $fixture 'src\bin') | Out-Null
    'excluded' | Set-Content (Join-Path $fixture 'src\bin\build-output.txt')
    foreach ($name in @('alpha', 'bravo')) {
        & (Join-Path $fixture 'scripts\create_all_settingsfiles_mi_only.ps1') -envName $name -sbmExe Invoke-ConfigSbm
        & (Join-Path $fixture 'scripts\Database\create_database_override_files.ps1') -envName $name
        & (Join-Path $fixture 'scripts\Database\create_pg_database_override_files.ps1') -envName $name
        & (Join-Path $fixture 'scripts\Database\create_mysql_database_override_files.ps1') -envName $name
        $directory = Get-TestConfigPath -envName $name -repoRoot $fixture
        Assert-Config (@(Get-ChildItem $directory -Filter 'settingsfile-*.json').Count -eq 5) 'Expected all five settings files.'
        foreach ($file in @('databasetargets.cfg', 'pg-databasetargets.cfg', 'mysql-databasetargets.cfg', 'server.txt', 'pg-server.txt', 'mysql-server.txt')) {
            Assert-Config ((Get-Content (Join-Path $directory $file) -Raw).Contains($name)) "'$file' has the wrong environment."
        }
        Assert-Config ((Get-Content (Join-Path $directory 'pg-pw.txt') -Raw).Trim() -eq "$name-PG_ADMIN_PASSWORD") 'PG credential came from another environment.'
    }
    $alpha = Get-TestConfigPath -envName alpha -repoRoot $fixture
    $bravo = Get-TestConfigPath -envName bravo -repoRoot $fixture
    Assert-Config ((Get-TestConfigPath -envName alpha) -eq $alpha) 'Default root must resolve relative to the helper, not the caller.'
    $alphaKey = Get-Content (Join-Path $alpha 'settingsfilekey.txt') -Raw
    Assert-Config ($alphaKey -ne (Get-Content (Join-Path $bravo 'settingsfilekey.txt') -Raw)) 'Environment keys must be distinct.'
    & (Join-Path $fixture 'scripts\key_file_names.ps1') -envName alpha
    Assert-Config ($alphaKey -eq (Get-Content (Join-Path $alpha 'settingsfilekey.txt') -Raw)) 'Rerun changed the selected key.'
    Assert-Config (@(Get-ChildItem $configRoot -File).Count -eq 1) 'A producer wrote to the legacy root.'

    $custom = Join-Path $fixture 'custom exact output'
    & (Join-Path $fixture 'scripts\create_all_settingsfiles_mysql_password.ps1') -envName alpha -path $custom -sbmExe Invoke-ConfigSbm
    & (Join-Path $fixture 'scripts\Database\create_mysql_database_override_files.ps1') -envName alpha -path $custom -authenticationMode Password
    Assert-Config (Test-Path (Join-Path $custom 'settingsfile-aci-mysql-password.json')) 'Explicit output path was not honored.'
    Assert-Config (-not (Test-Path (Join-Path $custom 'alpha'))) 'Environment was appended twice to an explicit override.'
    Assert-Config ((Get-Content (Join-Path $custom 'mysql-pw.txt') -Raw).Trim() -eq 'alpha-MYSQL_ADMIN_PASSWORD') 'MySQL credential came from another environment.'
    Assert-ConfigFailure { & (Join-Path $fixture 'scripts\Database\create_mysql_database_override_files.ps1') -envName alpha -path $custom } 'Stale Azure MySQL password'
    & (Join-Path $fixture 'scripts\Database\create_mysql_database_override_files.ps1') -envName bravo
    foreach ($invalid in @('', '..', '../alpha', 'alpha\bravo', 'C:\temp', 'UPPER')) {
        Assert-ConfigFailure { Get-TestConfigPath -envName $invalid -repoRoot $fixture -Create } 'Specify an azd environment'
    }
    Assert-ConfigFailure { Get-TestConfigPath -envName missing -repoRoot $fixture } 'does not exist'

    foreach ($name in @('alpha', 'bravo')) {
        $directory = Get-TestConfigPath -envName $name -repoRoot $fixture
        $state.SourceDirty = $name -eq 'bravo'
        New-Item -ItemType Directory -Path (Join-Path $directory 'TestResults') -Force | Out-Null
        'result' | Set-Content (Join-Path $directory 'TestResults\stale-result.txt')
        'bundle' | Set-Content (Join-Path $directory 'bundle.zip')
        $state.ExpectedFiles = @(Get-ChildItem $directory -File | Where-Object Extension -in @('.json', '.cfg', '.txt', '.yaml', '.yml')).Count
        & (Join-Path $fixture 'scripts\ContainerRegistry\build_external_test_image.ps1') -envName $name -resourceGroupName rg-explicit -imageTag isolated
    }
    Assert-Config ($state.Builds -eq 2) 'Both selected environments must reach mocked ACR build.'
    $state.ExpectedFiles = @(Get-ChildItem $custom -File | Where-Object Extension -in @('.json', '.cfg', '.txt', '.yaml', '.yml')).Count
    & (Join-Path $fixture 'scripts\ContainerRegistry\build_external_test_image.ps1') -envName alpha -path $custom -resourceGroupName rg-explicit -imageTag isolated
    $state.BuildExitCode = 1
    Assert-ConfigFailure {
        & (Join-Path $fixture 'scripts\ContainerRegistry\build_external_test_image.ps1') -envName alpha -path $custom -resourceGroupName rg-explicit -imageTag isolated
    } 'Failed to build test container image'
    $state.BuildExitCode = 0
    foreach ($context in $state.Contexts) {
        Assert-Config (-not (Test-Path -LiteralPath $context)) 'Image staging directory was not cleaned.'
    }
    Assert-ConfigFailure { & (Join-Path $fixture 'scripts\ContainerRegistry\build_external_test_image.ps1') -envName missing } 'does not exist'
    $empty = Get-TestConfigPath -envName empty -repoRoot $fixture -Create
    Assert-ConfigFailure { & (Join-Path $fixture 'scripts\ContainerRegistry\build_external_test_image.ps1') -envName empty } 'Missing settingsfilekey'
    Assert-Config ($state.Builds -eq 4) 'Missing environment or key must fail before uploading.'
    . (Join-Path $sourceRoot 'scripts\tests\aci_test_helpers.ps1')
    $downloaded = Download-TestResultsFromBlob -storageAccountName offline -localDestination (Join-Path $alpha 'TestResults') -envName bravo
    Assert-Config (-not $downloaded -and $state.RelayEnvironment -eq 'bravo') 'Relay fallback must use the requested environment and report missing configuration.'

    & {
        $mockSbm = Join-Path $fixture 'download-sbm.ps1'
        @'
$state.DownloadDestination = Get-Argument $args '--outputpath'
$global:LASTEXITCODE = 0
'@ | Set-Content -LiteralPath $mockSbm
        function Get-Command { param($Name, $ErrorAction); return @{ Source = $mockSbm } }
        function azd { $global:LASTEXITCODE = 0; return 'https://offline.servicebus.windows.net/relay' }
        function az {
            if ($viaRelay) { $global:LASTEXITCODE = 1; return 'Forbidden by network rules' }
            $state.DownloadDestination = Get-Argument $args '--destination'
            $global:LASTEXITCODE = 0
        }
        Push-Location $fixture
        try {
            foreach ($viaRelay in @($false, $true)) {
                $relative = Join-Path 'download results' $(if ($viaRelay) { 'relay' } else { 'direct' })
                $expected = Join-Path $fixture $relative
                $output = @(Download-TestResultsFromBlob -storageAccountName offline -localDestination $relative -envName alpha 6>&1)
                Assert-Config ($output[-1] -eq $true) 'Mocked result download should succeed.'
                $messages = @($output | Where-Object { $_ -is [System.Management.Automation.InformationRecord] } | ForEach-Object { $_.MessageData.ToString() })
                $success = if ($viaRelay) { 'Test results downloaded successfully through RelayProxy.' } else { 'Test results downloaded successfully.' }
                $index = [Array]::IndexOf($messages, $success)
                Assert-Config ($index -ge 0 -and $messages[$index + 1] -eq "  Results folder: $expected") 'Each download success message must be followed by the absolute results folder.'
                Assert-Config ($state.DownloadDestination -eq $expected) 'Reported results folder must match the actual download destination.'
            }
        } finally {
            Pop-Location
        }
    }

    # Exercise the production targets with real MSBuild but no package restore or databases.
    $project = Join-Path $fixture 'src\ConfigFixture.proj'
    @'
<Project>
  <PropertyGroup><OutDir>$(MSBuildThisFileDirectory)out\</OutDir></PropertyGroup>
  <Import Project="AzureTestConfig.targets" />
  <Target Name="GetCopyToOutputDirectoryItems" />
  <Target Name="CopyConfig" DependsOnTargets="ValidateAzureTestConfig;GetCopyToOutputDirectoryItems">
    <Copy SourceFiles="@(Content)" DestinationFiles="@(Content->'$(OutDir)%(Link)')" />
  </Target>
  <Target Name="VSTest" />
</Project>
'@ | Set-Content $project
    'alpha-only' | Set-Content (Join-Path $alpha 'alpha-only.txt')
    $output = Join-Path $fixture 'src\out\TestConfig'
    foreach ($name in @('alpha', 'bravo')) {
        $result = dotnet msbuild $project -t:CopyConfig "-p:AzdEnvironment=$name" -verbosity:quiet 2>&1
        Assert-Config ($LASTEXITCODE -eq 0) "MSBuild configuration copy failed: $result"
        Assert-Config ((Get-Content (Join-Path $output 'server.txt') -Raw).Contains($name)) 'Build output used the wrong environment.'
        Assert-Config (-not (Test-Path (Join-Path $output 'legacy.txt'))) 'Build output fell back to legacy files.'
        Assert-Config (@(Get-ChildItem $output -Directory).Count -eq 0) 'Build output copied nested environments or results.'
    }
    Assert-Config (-not (Test-Path (Join-Path $output 'alpha-only.txt'))) 'Switching environments left stale configuration.'
    $env:AZURE_ENV_NAME = 'alpha'
    $result = dotnet msbuild $project -t:CopyConfig -verbosity:quiet 2>&1
    Assert-Config ($LASTEXITCODE -eq 0 -and (Test-Path (Join-Path $output 'alpha-only.txt'))) "AZURE_ENV_NAME selection failed: $result"
    $result = dotnet msbuild $project -t:VSTest -p:VSTestNoBuild=true -p:AzdEnvironment=bravo -verbosity:quiet 2>&1
    Assert-Config ($LASTEXITCODE -eq 0) "No-build staging failed: $result"
    Assert-Config (-not (Test-Path (Join-Path $output 'alpha-only.txt'))) 'No-build switch retained stale configuration.'
    Assert-Config ((Get-Content (Join-Path $output 'server.txt') -Raw).Contains('bravo')) 'Explicit selection did not override AZURE_ENV_NAME in no-build tests.'
    $sourceOutput = Join-Path $fixture 'src\'
    $result = dotnet msbuild $project -t:CopyConfig "-p:OutDir=$sourceOutput" -verbosity:quiet 2>&1
    Assert-Config ($LASTEXITCODE -ne 0) 'Source TestConfig directory was accepted as an output directory.'
    Assert-Config (Test-Path (Join-Path $alpha 'settingsfilekey.txt')) 'Output cleanup deleted source configuration.'
    $env:AZURE_ENV_NAME = ''
    foreach ($properties in @('-p:AzdEnvironment=missing', '-p:AzdEnvironment=../alpha', '-p:AzdEnvironment=')) {
        $result = dotnet msbuild $project -t:VSTest $properties -verbosity:quiet 2>&1
        Assert-Config ($LASTEXITCODE -ne 0) "MSBuild accepted an invalid or missing selection: $properties"
    }
    $result = dotnet msbuild $project -t:CopyConfig -verbosity:quiet 2>&1
    Assert-Config ($LASTEXITCODE -eq 0 -and -not (Test-Path $output)) "Unselected build retained stale settings: $result"
    if ($BuildAzureProjects) {
        foreach ($platform in @('MySQL', 'PostgreSQL', 'SqlServer')) {
            $azureProject = Join-Path $sourceRoot "src\SqlBuildManager.Console.$platform.AzureTest\SqlBuildManager.Console.$platform.AzureTest.csproj"
            $buildOutput = Join-Path $fixture "build-$platform\"
            $buildConfig = Join-Path $buildOutput 'TestConfig'
            $result = dotnet build $azureProject --no-restore -v:q "-p:OutDir=$buildOutput" "-p:_AzureTestConfigDirectory=$alpha" -p:AzdEnvironment=alpha 2>&1
            Assert-Config ($LASTEXITCODE -eq 0) "$platform build failed: $result"
            Assert-Config (Test-Path (Join-Path $buildConfig 'alpha-only.txt')) "$platform build did not copy selected configuration."
            $result = dotnet test $azureProject --no-build --no-restore --list-tests -v:q "-p:OutDir=$buildOutput" "-p:_AzureTestConfigDirectory=$bravo" -p:AzdEnvironment=bravo 2>&1
            Assert-Config ($LASTEXITCODE -eq 0) "$platform no-build discovery failed: $result"
            Assert-Config ((Get-Content (Join-Path $buildConfig 'server.txt') -Raw).Contains('bravo')) "$platform no-build run retained the previous environment."
            Assert-Config (-not (Test-Path (Join-Path $buildConfig 'alpha-only.txt'))) "$platform no-build run retained stale files."
            $result = dotnet test $azureProject --no-build --no-restore --list-tests -v:q "-p:OutDir=$buildOutput" 2>&1
            Assert-Config ($LASTEXITCODE -ne 0 -and "$result".Contains('Select Azure test configuration')) "$platform accepted no-build tests without an environment: $result"
            Write-Output "PASS: $platform real build and no-build discovery isolation."
        }
    }
    Assert-Config ((Get-Content (Join-Path $configRoot 'legacy.txt') -Raw).Trim() -eq 'legacy-root-sentinel') 'Legacy source files were changed.'
    Write-Output "PASS: $($state.Checks) environment configuration assertions (no Azure calls)."
}
finally {
    $env:AZD_PROJECT_PATH = $savedProjectPath
    $env:AZURE_ENV_NAME = $savedEnvironment
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
