[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$runner = Join-Path $PSScriptRoot 'run_filtered_external_tests_in_aci.ps1'
$tokens = $null
$parseErrors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($runner, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -ne 0) {
    throw "Test runner has PowerShell syntax errors: $($parseErrors.Message -join '; ')"
}

function Get-RunnerAssignment([string] $Name) {
    $assignments = @($ast.FindAll({
        param($node)
        $node -is [Management.Automation.Language.AssignmentStatementAst] -and
        $node.Left -is [Management.Automation.Language.VariableExpressionAst] -and
        $node.Left.VariablePath.UserPath -eq $Name -and
        $node.Operator -eq [Management.Automation.Language.TokenKind]::Equals
    }, $true))
    if ($assignments.Count -ne 1) {
        throw "Expected one '$Name' template assignment, got $($assignments.Count)."
    }
    & ([scriptblock]::Create($assignments[0].Right.Extent.Text))
}

$identity = [pscustomobject]@{
    id = '/subscriptions/test/resourceGroups/rg-test/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id-test-orchestrator'
    clientId = '11111111-1111-1111-1111-111111111111'
}
$workerIdentity = [pscustomobject]@{
    id = '/subscriptions/test/resourceGroups/rg-test/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id-test-worker'
    clientId = '22222222-2222-2222-2222-222222222222'
}
$location = 'test-region'
$testContainerName = 'test-runner'
$acrLoginServer = 'test.azurecr.io'
$fullImageName = 'test.azurecr.io/sqlbuildmanager-tests:test-runner'
$subnetId = '/subscriptions/test/resourceGroups/rg-test/providers/Microsoft.Network/virtualNetworks/test/subnets/aci'
$aksPreCmd = ''
$testCmd = 'dotnet vstest test.dll | tee results.log'
$uploadCmd = 'az storage blob upload-batch --auth-mode login'

$shellCmd = Get-RunnerAssignment 'shellCmd'
$commandYaml = Get-RunnerAssignment 'commandYaml'
$envVarsYaml = Get-RunnerAssignment 'envVarsYaml'
$aciYaml = Get-RunnerAssignment 'aciYaml'

if ($envVarsYaml -notmatch "name: SBM_ORCHESTRATOR_CLIENT_ID\s+value: $($identity.clientId)" -or
    $envVarsYaml -notmatch "name: AZURE_CLIENT_ID\s+value: $($identity.clientId)" -or
    $envVarsYaml.Contains($workerIdentity.clientId)) {
    throw 'Control-plane authentication must explicitly select the orchestrator, not the database worker.'
}
foreach ($resourceId in @($identity.id, $workerIdentity.id)) {
    if (-not $aciYaml.Contains("${resourceId}: {}")) {
        throw "Trusted test runner is missing required identity '$resourceId'."
    }
}
if ($shellCmd -notmatch 'az login --identity --client-id \$AZURE_CLIENT_ID --output none \|\| exit \$\?' -or
    $shellCmd -notmatch 'TEST_EXIT_CODE=\$\{PIPESTATUS\[0\]\}' -or
    $shellCmd -notmatch 'TEST_EXIT_CODE=\$UPLOAD_EXIT_CODE') {
    throw 'Authentication, test execution or upload failures could be hidden.'
}
if ($aciYaml -match 'postprovision|mysql-directory|aks-control|aks-kubelet') {
    throw 'A privileged infrastructure identity must not be attached to the test runner.'
}

Write-Output 'ACI test-runner identity rendering checks passed (no Azure calls).'
