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
$results = Join-Path $repoRoot "testresults"
$exitCode = 1
New-Item -ItemType Directory -Force -Path $results | Out-Null

$env:SBM_TEST_PLATFORM = $Platform
if ($Filter) { $env:TEST_FILTER = $Filter }
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
    & docker compose -p $projectName -f $composeFile @profiles up --build --abort-on-container-exit --exit-code-from "test-$Platform"
    $exitCode = $LASTEXITCODE
}
finally {
    & docker compose -p $projectName -f $composeFile @profiles logs --no-color 2>&1 |
        Tee-Object -FilePath (Join-Path $results "docker-compose-$Platform.log")
    & docker compose -p $projectName -f $composeFile @profiles down --volumes --remove-orphans
}
exit $exitCode
