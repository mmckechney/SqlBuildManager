<#
.SYNOPSIS
    Deletes generated ACI JSON definition files from ExternalTest build output.
.DESCRIPTION
    Removes aci-*.json files created during ACI integration test runs from the
    ExternalTest project's build output directory.
#>
Write-Host "Deleting ACI unit test json files" -ForegroundColor Green

$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$projectPath = Join-Path $repoRoot "src\SqlBuildManager.Console.SqlServer.AzureTest\SqlBuildManager.Console.SqlServer.AzureTest.csproj"
$frameworkTarget = (Select-Xml -Path $projectPath -XPath "/Project/PropertyGroup/TargetFramework").Node.InnerText
$outputPath = Join-Path $repoRoot "src\SqlBuildManager.Console.SqlServer.AzureTest\bin\Debug\$frameworkTarget"
if (Test-Path $outputPath) {
    Get-ChildItem $outputPath -Filter aci-*.json -Recurse -Force | Remove-Item -Force
}