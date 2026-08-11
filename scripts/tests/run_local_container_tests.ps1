[CmdletBinding()]
param(
    [ValidateSet("sqlserver", "postgresql", "mysql")]
    [string] $Platform = "sqlserver",
    [string] $Filter,
    [switch] $IncludeEmulators
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$composeFile = Join-Path $PSScriptRoot "local-container\docker-compose.yml"
$projectName = "sbm-local-$Platform"
$runtimeImage = "sbm-local-runtime"
$testImage = "sbm-local-test-$Platform"
$runtimeNetwork = "${projectName}_default"
$resultsVolume = "${projectName}-test-results"
$results = Join-Path $PSScriptRoot "testresults"
$runResults = Join-Path $results ("{0}-{1}" -f $Platform, (Get-Date -Format "yyyyMMdd-HHmmss"))
$exitCode = 1
New-Item -ItemType Directory -Force -Path $results | Out-Null
New-Item -ItemType Directory -Force -Path $runResults | Out-Null

$env:SBM_TEST_PLATFORM = $Platform
$env:SBM_LOCAL_TEST_RESULTS = $runResults
$env:SBM_RUNTIME_IMAGE = $runtimeImage
$env:SBM_TEST_IMAGE = $testImage
$env:SBM_RUNTIME_NETWORK = $runtimeNetwork
$env:SBM_TEST_RESULTS_VOLUME = $resultsVolume
if ($Filter) { $env:TEST_FILTER = $Filter }
if ($Filter -match "RuntimeContainer") { $env:SBM_RUNTIME_CONTAINER_MODE = "true" }
if ($IncludeEmulators) {
    # These values are resolved on the Compose network, not from the host.
    $env:SBM_TEST_BLOB_ENDPOINT = "http://azurite:10000/devstoreaccount1"
    $env:SBM_BLOB_ENDPOINT = $env:SBM_TEST_BLOB_ENDPOINT
    $env:SBM_TEST_EVENTHUB_NAME = "sbm-events"
    $env:SBM_TEST_EVENTHUB_CONNECTION_STRING = "Endpoint=sb://eventhubs-emulator/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;EntityPath=sbm-events;UseDevelopmentEmulator=true"
    $env:SBM_TEST_SERVICEBUS_CONNECTION_STRING = "Endpoint=sb://servicebus-emulator:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true"
} else {
    Remove-Item Env:SBM_TEST_BLOB_ENDPOINT, Env:SBM_BLOB_ENDPOINT, Env:SBM_TEST_EVENTHUB_NAME, Env:SBM_TEST_EVENTHUB_CONNECTION_STRING, Env:SBM_TEST_SERVICEBUS_CONNECTION_STRING -ErrorAction SilentlyContinue
}
$profiles = @("--profile", $Platform)
if ($IncludeEmulators) { $profiles += @("--profile", "emulators") }

try {
    & docker compose -p $projectName -f $composeFile @profiles config --quiet
    if ($LASTEXITCODE -ne 0) { throw "docker compose config validation failed." }
    & docker image inspect $runtimeImage *> $null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Building missing production runtime image: $runtimeImage"
        & docker compose -p $projectName -f $composeFile build runtime-image
        if ($LASTEXITCODE -ne 0) { throw "The production runtime image build failed." }
    }
    else {
        Write-Host "Using existing production runtime image: $runtimeImage"
    }

    & docker image inspect $testImage *> $null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Building missing test-runner image: $testImage"
        & docker compose -p $projectName -f $composeFile @profiles build "test-$Platform"
        if ($LASTEXITCODE -ne 0) { throw "The test-runner image build failed." }
    }
    else {
        Write-Host "Using existing test-runner image: $testImage"
    }

    & docker compose -p $projectName -f $composeFile @profiles up --abort-on-container-exit --exit-code-from "test-$Platform"
    $exitCode = $LASTEXITCODE
}
finally {
    & docker compose -p $projectName -f $composeFile @profiles logs --no-color 2>&1 |
        Tee-Object -FilePath (Join-Path $runResults "docker-compose-$Platform.log")
    # The test containers share a named volume with sibling runtime containers.
    # Copy it to the host before compose down removes the volume.
    & docker run --rm `
        -v "${resultsVolume}:/source" `
        -v "${runResults}:/destination" `
        busybox sh -c "cp -a /source/. /destination/" 2>&1 |
        Tee-Object -FilePath (Join-Path $runResults "runtime-container-volume-copy.log")
    & docker compose -p $projectName -f $composeFile @profiles down --volumes --remove-orphans
    Remove-Item Env:SBM_LOCAL_TEST_RESULTS -ErrorAction SilentlyContinue
    Remove-Item Env:SBM_RUNTIME_IMAGE, Env:SBM_TEST_IMAGE, Env:SBM_RUNTIME_NETWORK, Env:SBM_TEST_RESULTS_VOLUME -ErrorAction SilentlyContinue
    Remove-Item Env:SBM_RUNTIME_CONTAINER_MODE -ErrorAction SilentlyContinue
}
exit $exitCode
