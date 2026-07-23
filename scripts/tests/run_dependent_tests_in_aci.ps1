 param
(
    [Parameter()]
    [string] $envName,

    [string] $resourceGroupName,
    [string] $customName = "dependent",
    
    [string] $testFilter = "",
    
    [string] $imageTag = "dependent-test-runner",
    
    # No defaults: caller must supply or the script generates cryptographically random values.
    [string] $sqlPassword = "",

    [string] $pgPassword = "",

    [string] $mySqlPassword = "",

    [switch] $buildImage,
    
    [switch] $keepContainer,
    
    [int] $timeoutMinutes = 300,

    [string] $timestamp = (Get-Date -Format 'yyyy-MM-dd-HHmmss')
)

<#
.SYNOPSIS
    Runs Dependent.UnitTest integration tests in ACI with SQL Server, PostgreSQL, and MySQL sidecars.

.DESCRIPTION
    Deploys a container group to ACI with four containers:
    1. SQL Server 2022 on Linux (sidecar)
    2. PostgreSQL 16 (sidecar)
    3. MySQL 8.4 (sidecar)
    4. Test runner - runs the Dependent.UnitTest projects against all three sidecars
    
    The test runner waits for database sidecars to be ready, then runs all Dependent.UnitTest
    projects. SqlSync.SqlBuild.Dependent.UnitTest runs first to create the test databases.
    
    Environment variables SBM_TEST_SQL_SERVER, SBM_TEST_SQL_USER, and SBM_TEST_SQL_PASSWORD
    are set automatically to connect to the sidecar SQL Server instance.

.PARAMETER envName
    The Azure Developer CLI environment name used when deploying resources.

.PARAMETER sqlPassword
    SA password for the SQL Server sidecar (must meet SQL Server complexity requirements).

.PARAMETER buildImage
    If specified, builds and pushes the test container image before running.

.PARAMETER keepContainer
    If specified, keeps the ACI container group after test completion for debugging.

.EXAMPLE
    # Build image and run all dependent tests
    .\run_dependent_tests_in_aci.ps1 -envName mwm025 -buildImage

.EXAMPLE
    # Run with custom SQL password
    .\run_dependent_tests_in_aci.ps1 -envName mwm025 -sqlPassword "MyStr0ng!Pass"
#>

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Generate cryptographically random test passwords when none are supplied.
# In CI the workflow always passes explicit values; this covers local interactive use.
function New-SecureTestPassword {
    param([string] $tag)
    $bytes = [System.Security.Cryptography.RandomNumberGenerator]::GetBytes(18)
    $b64   = [Convert]::ToBase64String($bytes).Replace('+','P').Replace('/','Q').Replace('=','R')
    # Guarantee complexity: tag provides upper+special+digit, b64 adds alphanumeric variety.
    return "${tag}1!${b64}".Substring(0, 24)
}
if ([string]::IsNullOrWhiteSpace($sqlPassword)) { $sqlPassword = New-SecureTestPassword -tag 'Sql' }
if ([string]::IsNullOrWhiteSpace($pgPassword))  { $pgPassword  = New-SecureTestPassword -tag 'Pg'  }
if ([string]::IsNullOrWhiteSpace($mySqlPassword))  { $mySqlPassword  = New-SecureTestPassword -tag 'My'  }

# Resolve environment name: parameter > azd environment
if ([string]::IsNullOrWhiteSpace($envName)) {
    $envName = azd env get-value AZURE_ENV_NAME 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($envName)) {
        $envName = $null
    } else {
        Write-Host "Using environment '$envName' from AZURE_ENV_NAME" -ForegroundColor DarkGreen
    }
}

if ([string]::IsNullOrWhiteSpace($envName)) {
    Write-Host "ERROR: The -envName parameter is required." -ForegroundColor Red
    Write-Host "  Provide it as a parameter:  .\run_dependent_tests_in_aci.ps1 -envName <your-env>" -ForegroundColor Yellow
    Write-Host "  Or select an azd environment with 'azd env select'." -ForegroundColor Yellow
    exit 1
}

# Suppress Clear-Host in non-interactive (CI) environments.
if ($Host.Name -eq 'ConsoleHost' -and [string]::IsNullOrWhiteSpace($env:CI)) { Clear-Host }

# Dot-source shared ACI test helpers
. (Join-Path $PSScriptRoot "aci_test_helpers.ps1")
Initialize-TestSummaryState

# Get the repo root — use $PSScriptRoot for reliable portable resolution.
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
}

#############################################
# Get resource name variables from the environment name
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
$resourceGroupNameOverride = $resourceGroupName
. $prefixScript -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
}

$testContainerName = "$aciName-test-runner-$customName"
$testImageName = "sqlbuildmanager-dependent-tests"

Write-Host ""
Write-Host "=======================================================================" -ForegroundColor Cyan
Write-Host "Unit Test and Local DB Test Runner (ACI with SQL, PostgreSQL, and MySQL Sidecars)" -ForegroundColor Cyan
Write-Host "=======================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Container Name: $testContainerName" -ForegroundColor DarkGreen
if ($testFilter) {
    Write-Host "Test Filter: $testFilter" -ForegroundColor DarkGreen
}
Write-Host ""

# Get resource information
$subscriptionId = az account show --query id --output tsv
if ($LASTEXITCODE -ne 0) { throw "az account show failed (exit code $LASTEXITCODE). Ensure you are logged in." }
$identity = az identity show --resource-group $resourceGroupName --name $identityName | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw "az identity show failed for '$identityName' (exit code $LASTEXITCODE)." }
$acrLoginServer = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer
if ($LASTEXITCODE -ne 0) { throw "az acr show failed for '$containerRegistryName' (exit code $LASTEXITCODE)." }

Write-Host "Using Managed Identity: $identityName (ClientId: $($identity.clientId))" -ForegroundColor DarkGreen
Write-Host "Using Container Registry: $acrLoginServer" -ForegroundColor DarkGreen
Write-Host ""

#############################################
# Build and push test image if requested
#############################################
if ($buildImage) {
    $buildScript = Join-Path $repoRoot "scripts\ContainerRegistry\build_dependent_test_image.ps1"
    & $buildScript -envName $envName -resourceGroupName $resourceGroupName -imageTag $imageTag
    if ($LASTEXITCODE -ne 0) { throw "build_dependent_test_image.ps1 failed (exit code $LASTEXITCODE)." }
}

#############################################
# Clean up any existing test container
#############################################
Remove-ExistingAciContainer -containerName $testContainerName -resourceGroupName $resourceGroupName

#############################################
# Get subnet ID for VNet deployment
#############################################
$subnetId = Get-AciSubnetId -resourceGroupName $resourceGroupName -vnetName $vnet -subnetName $aciSubnet

#############################################
# Build container commands
#############################################
$blobContainerName = "testresults"

$blobPath = "$timestamp/$testContainerName"

$uploadCmd = "az storage blob upload-batch --account-name $storageAccountName --destination $blobContainerName --source /tests/TestResults --destination-path $blobPath --auth-mode login --overwrite"

# Test runner: login to Azure, run tests, upload results
$testShellCmd = "az login --identity --client-id `$AZURE_CLIENT_ID; /tests/run-tests.sh; TEST_EXIT_CODE=`$?; echo TEST_EXIT_CODE=`$TEST_EXIT_CODE; $uploadCmd; exit `$TEST_EXIT_CODE"

$fullImageName = "$acrLoginServer/${testImageName}:${imageTag}"

#############################################
# Deploy container group with SQL sidecar
#############################################
Write-Debug "========================================" 
Write-Debug "Deploying Container Group to ACI" 
Write-Debug "========================================" 
Write-Debug "Test Image: $fullImageName" 
Write-Debug "SQL Server: mcr.microsoft.com/mssql/server:2022-latest" 
Write-Debug "PostgreSQL: docker.io/library/postgres:16" 
Write-Debug "MySQL: docker.io/library/mysql:8.4"
Write-Debug ""

$location = az group show --name $resourceGroupName --query location -o tsv
if ($LASTEXITCODE -ne 0) { throw "az group show failed for '$resourceGroupName' (exit code $LASTEXITCODE)." }

# Build environment variables for test filter
$testFilterEnvVar = ""
if ($testFilter) {
    $testFilterEnvVar = @"

      - name: TEST_FILTER
        value: $testFilter
"@
}

$aciYaml = @"
apiVersion: 2021-09-01
location: $location
name: $testContainerName
identity:
  type: UserAssigned
  userAssignedIdentities:
    $($identity.id): {}
properties:
  imageRegistryCredentials:
  - server: $acrLoginServer
    identity: $($identity.id)
  containers:
  - name: sql-server
    properties:
      image: mcr.microsoft.com/mssql/server:2022-latest
      environmentVariables:
      - name: ACCEPT_EULA
        value: "Y"
      - name: MSSQL_SA_PASSWORD
        value: "$sqlPassword"
      - name: MSSQL_DATA_DIR
        value: "/var/opt/mssql/data"
      resources:
        requests:
          cpu: 2
          memoryInGb: 4
      ports:
      - port: 1433
  - name: postgres-server
    properties:
      image: docker.io/library/postgres:16
      environmentVariables:
      - name: POSTGRES_USER
        value: "postgres"
      - name: POSTGRES_PASSWORD
        value: "$pgPassword"
      resources:
        requests:
          cpu: 1
          memoryInGb: 2
      ports:
      - port: 5432
  - name: mysql-server
    properties:
      image: docker.io/library/mysql:8.4
      environmentVariables:
      - name: MYSQL_ROOT_PASSWORD
        value: "$mySqlPassword"
      - name: MYSQL_ROOT_HOST
        value: "%"
      resources:
        requests:
          cpu: 1
          memoryInGb: 2
      ports:
      - port: 3306
  - name: test-runner
    properties:
      image: $fullImageName
      command:
      - /bin/bash
      - -c
      - "$testShellCmd"
      environmentVariables:
      - name: AZURE_CLIENT_ID
        value: $($identity.clientId)
      - name: SBM_TEST_SQL_SERVER
        value: localhost
      - name: SBM_TEST_SQL_USER
        value: sa
      - name: SBM_TEST_SQL_PASSWORD
        value: "$sqlPassword"
      - name: SBM_TEST_DB_PATH
        value: "/var/opt/mssql/data"
      - name: SBM_TEST_POSTGRES_SERVER
        value: localhost
      - name: SBM_TEST_POSTGRES_USER
        value: postgres
      - name: SBM_TEST_POSTGRES_PASSWORD
        value: "$pgPassword"
      - name: SBM_TEST_MYSQL_SERVER
        value: localhost
      - name: SBM_TEST_MYSQL_USER
        value: root
      - name: SBM_TEST_MYSQL_PASSWORD
        value: "$mySqlPassword"$testFilterEnvVar
      resources:
        requests:
          cpu: 2
          memoryInGb: 4
  osType: Linux
  restartPolicy: Never
  subnetIds:
  - id: $subnetId
"@

# Deploy container group via shared helper
Deploy-AciFromYaml -yamlContent $aciYaml -resourceGroupName $resourceGroupName -yamlFilePrefix "aci-dependent-tests"

#############################################
# Wait for tests to complete
#############################################
$monitorResult = Wait-ForAciTests `
    -containerName $testContainerName `
    -resourceGroupName $resourceGroupName `
    -timeoutMinutes $timeoutMinutes `
    -logContainerName "test-runner" `
    -keepContainer:$keepContainer `
    -sqlContainerName "sql-server" `
    -testFilter $testFilter `
    -imageName $fullImageName

$testExitCode = $monitorResult.TestExitCode

#############################################
# Results
#############################################
# Write-Host ""
# Write-Host "========================================" -ForegroundColor Cyan
# Write-Host "Test Results" -ForegroundColor Cyan
# Write-Host "========================================" -ForegroundColor Cyan

# if ($null -eq $testExitCode) {
#     $testExitCode = 1
# }

# Write-Host ""
# Write-Host "Test Exit Code: $testExitCode" -ForegroundColor DarkGreen
# Write-Host "Results uploaded to: $blobContainerName/$blobPath" -ForegroundColor Cyan
# Write-Host ""

# # Get and parse full test runner logs
# $fullTestLogs = Get-AciContainerLogs -containerName $testContainerName -resourceGroupName $resourceGroupName -logContainerName "test-runner"
# if ($fullTestLogs) {
#     Show-TestSummary -logs $fullTestLogs -startTime $startTime
# }
# else {
#     Write-Host "No test logs available" -ForegroundColor Yellow
# }

# Download test results from blob storage
Download-TestResultsFromBlob `
    -storageAccountName $storageAccountName `
    -blobContainerName $blobContainerName `
    -localDestination "./testresults" `
    -blobPath $blobPath | Out-Null

#############################################
# Cleanup
#############################################
$finalExitCode = Complete-AciTestRun -containerName $testContainerName -resourceGroupName $resourceGroupName -exitCode $testExitCode -keepContainer:$keepContainer -logContainerName "test-runner" -sqlContainerName "sql-server"

# Analyze test results with GitHub Copilot CLI (local developer convenience; skip in CI).
if (Get-Command copilot -ErrorAction SilentlyContinue) {
    $promptTemplate = Get-Content -Path (Join-Path $PSScriptRoot 'analyze-test-results-prompt.md') -Raw
    $prompt = $promptTemplate -replace '\{\{timestamp\}\}', $timestamp
    $analysis = copilot --yolo -p $prompt 2>&1
    Write-Host "Analysis complete." -ForegroundColor Cyan
}

exit $finalExitCode