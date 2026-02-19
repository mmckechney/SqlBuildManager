 param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $resourceGroupName,
    [string] $customName = "dependent",
    
    [string] $testFilter = "",
    
    [string] $imageTag = "dependent-test-runner",
    
    [string] $sqlPassword = "SqlBM_Test#2026!",

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
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Building Dependent Test Container Image" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $srcPath = Join-Path $repoRoot "src"
    
    # Build using ACR (same pattern as build_container_registry_testimage.ps1)
    $tmpDir = Join-Path $env:TEMP "sbm-dependent-tests-$(Get-Date -Format 'yyyyMMddHHmmss')"
    New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null
    
    # Copy source, excluding build artifacts
    Write-Host "Copying source files..." -ForegroundColor DarkGreen
    $excludeDirs = @('.vs', 'bin', 'obj', 'TestResults')
    Get-ChildItem -Path $srcPath -Recurse -File | Where-Object {
        $relativePath = $_.FullName.Substring($srcPath.Length)
        $excluded = $false
        foreach ($dir in $excludeDirs) {
            if ($relativePath -like "*\$dir\*" -or $relativePath -like "*/$dir/*") {
                $excluded = $true
                break
            }
        }
        -not $excluded
    } | ForEach-Object {
        $destPath = Join-Path $tmpDir $_.FullName.Substring($srcPath.Length)
        $destDir = Split-Path $destPath -Parent
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }
        Copy-Item $_.FullName -Destination $destPath
    }
    
    # Remove .dockerignore if present
    $dockerIgnore = Join-Path $tmpDir ".dockerignore"
    if (Test-Path $dockerIgnore) { Remove-Item $dockerIgnore -Force }
    
    Write-Host "Building image via ACR..." -ForegroundColor DarkGreen
    az acr build --registry $containerRegistryName `
        --resource-group $resourceGroupName `
        --image "${testImageName}:${imageTag}" `
        --file (Join-Path $tmpDir "Dockerfile.dependent-tests") `
        $tmpDir
    
    # Cleanup
    $oldProgress = $ProgressPreference
    $ProgressPreference = 'SilentlyContinue'
    Remove-Item -Path $tmpDir -Recurse -Force -ErrorAction SilentlyContinue
    $ProgressPreference = $oldProgress
    Write-Host "Image built and pushed: ${testImageName}:${imageTag}" -ForegroundColor Green
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
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deploying Container Group to ACI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Image: $fullImageName" -ForegroundColor DarkGreen
Write-Host "SQL Server: mcr.microsoft.com/mssql/server:2022-latest" -ForegroundColor DarkGreen
Write-Host ""

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
        value: "/var/opt/mssql/data"$testFilterEnvVar
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
    -sqlContainerName "sql-server"
$testExitCode = $monitorResult.TestExitCode

#############################################
# Results
#############################################
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Results" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($null -eq $testExitCode) {
    $testExitCode = 1
}

Write-Host ""
Write-Host "Test Exit Code: $testExitCode" -ForegroundColor DarkGreen
Write-Host "Results uploaded to: $blobContainerName/$blobPath" -ForegroundColor Cyan
Write-Host ""

# Get and parse full test runner logs
$fullTestLogs = Get-AciContainerLogs -containerName $testContainerName -resourceGroupName $resourceGroupName -logContainerName "test-runner"
if ($fullTestLogs) {
    Show-TestSummary -logs $fullTestLogs -startTime $startTime
}
else {
    Write-Host "No test logs available" -ForegroundColor Yellow
}

# Download test results from blob storage
Download-TestResultsFromBlob -storageAccountName $storageAccountName -blobContainerName $blobContainerName -localDestination "./testresults" -blobPath $blobPath

#############################################
# Cleanup
#############################################
Complete-AciTestRun -containerName $testContainerName -resourceGroupName $resourceGroupName -exitCode $testExitCode -keepContainer:$keepContainer -logContainerName "test-runner" -sqlContainerName "sql-server"