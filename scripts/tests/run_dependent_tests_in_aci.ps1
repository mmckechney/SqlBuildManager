 param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $resourceGroupName,
    [string] $customName = "dependent",
    
    [string] $testFilter = "",
    
    [string] $imageTag = "dependent-test-runner",
    
    [string] $sqlPassword = "SqlBM_Test#2026!",

    [string] $pgPassword = "P0stSqlAdm1n",

    [switch] $buildImage,
    
    [switch] $keepContainer,
    
    [int] $timeoutMinutes = 300
)

<#
.SYNOPSIS
    Runs Dependent.UnitTest integration tests in ACI with a SQL Server sidecar.

.DESCRIPTION
    Deploys a container group to ACI with two containers:
    1. SQL Server 2022 on Linux (sidecar) - provides the database instance
    2. Test runner - runs the Dependent.UnitTest projects against the SQL Server sidecar
    
    The test runner waits for SQL Server to be ready, then runs all 5 Dependent.UnitTest
    projects. SqlSync.SqlBuild.Dependent.UnitTest runs first to create the test databases.
    
    Environment variables SBM_TEST_SQL_SERVER, SBM_TEST_SQL_USER, and SBM_TEST_SQL_PASSWORD
    are set automatically to connect to the sidecar SQL Server instance.

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER sqlPassword
    SA password for the SQL Server sidecar (must meet SQL Server complexity requirements).

.PARAMETER buildImage
    If specified, builds and pushes the test container image before running.

.PARAMETER keepContainer
    If specified, keeps the ACI container group after test completion for debugging.

.EXAMPLE
    # Build image and run all dependent tests
    .\run_dependent_tests_in_aci.ps1 -prefix mwm025 -buildImage

.EXAMPLE
    # Run with custom SQL password
    .\run_dependent_tests_in_aci.ps1 -prefix mwm025 -sqlPassword "MyStr0ng!Pass"
#>

$ErrorActionPreference = "Stop"

Clear-Host 

# Dot-source shared ACI test helpers
. (Join-Path $PSScriptRoot "aci_test_helpers.ps1")
Initialize-TestSummaryState

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

#############################################
# Get resource name variables from prefix
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -prefix $prefix

if ([string]::IsNullOrWhiteSpace($resourceGroupName)) {
    $resourceGroupName = "$prefix-rg"
}

$testContainerName = "$prefix-test-runner-$customName"
$testImageName = "sqlbuildmanager-dependent-tests"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Dependent Test Runner (ACI + SQL Sidecar)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Container Name: $testContainerName" -ForegroundColor DarkGreen
if ($testFilter) {
    Write-Host "Test Filter: $testFilter" -ForegroundColor DarkGreen
}
Write-Host ""

# Get resource information
$subscriptionId = az account show --query id --output tsv
$identity = az identity show --resource-group $resourceGroupName --name $identityName | ConvertFrom-Json
$acrLoginServer = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer

Write-Host "Using Managed Identity: $identityName (ClientId: $($identity.clientId))" -ForegroundColor DarkGreen
Write-Host "Using Container Registry: $acrLoginServer" -ForegroundColor DarkGreen
Write-Host ""

#############################################
# Build and push test image if requested
#############################################
if ($buildImage) {
    $buildScript = Join-Path $repoRoot "scripts\ContainerRegistry\build_container_registry_dependenttest_image.ps1"
    & $buildScript -prefix $prefix -resourceGroupName $resourceGroupName -imageTag $imageTag
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
$timestamp = Get-Date -Format 'yyyy-MM-dd-HHmmss'
$blobPath = "$testContainerName/$timestamp"

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
Write-Debug ""

$location = az group show --name $resourceGroupName --query location -o tsv

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
        value: "$pgPassword"$testFilterEnvVar
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
Download-TestResultsFromBlob -storageAccountName $storageAccountName -blobContainerName $blobContainerName -localDestination "./testresults" -blobPath $blobPath

#############################################
# Cleanup
#############################################
$finalExitCode = Complete-AciTestRun -containerName $testContainerName -resourceGroupName $resourceGroupName -exitCode $testExitCode -keepContainer:$keepContainer -logContainerName "test-runner" -sqlContainerName "sql-server"
exit $finalExitCode