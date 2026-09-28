<#
.SYNOPSIS
    Validates that every settings file in TestConfig is referenced by unit tests.
.DESCRIPTION
    Scans all .yaml and .json files in the TestConfig directory and checks that each
    is referenced somewhere in the ExternalTest project source. Reports any unreferenced
    settings files as errors.
.PARAMETER path
    Exact configuration directory override. Defaults to src\TestConfig\<envName>.
.PARAMETER envName
    Azure Developer CLI environment whose settings should be checked.
.PARAMETER testPath
    Path to the ExternalTest project directory. Default: ../../src/SqlBuildManager.Console.SqlServer.AzureTest.
#>
param
(
    [string] $path,
    [string] $testPath = (Join-Path $PSScriptRoot '..\..\src\SqlBuildManager.Console.SqlServer.AzureTest'),
    [string] $envName
)

. (Join-Path $PSScriptRoot '..\test_config_paths.ps1')
$path = Get-TestConfigPath -envName $envName -path $path
$testSettingsFiles = Get-ChildItem -LiteralPath $path -File
$haveMissing = $false
foreach($file in $testSettingsFiles)
{
    $name = $file.Name
    if($name.EndsWith("yaml") -or $name.EndsWith("json"))
    {
        Write-Host "Looking for refrences to: $name" -ForegroundColor Cyan
        $results = Get-ChildItem $testPath  -File | Select-String -pattern "/$name"
        if($null -ne $results)
        {
             Write-Host "$($results.Length) references found" -ForegroundColor Green   
             foreach($res in $results)
             {
               Write-Host $res.Filename "`t"  $res.LineNumber "`t" $res.Line.TrimStart()
             }
        }
        else
        {
            Write-Host "No reference found for settings file $name" -ForegroundColor Red
            $haveMissing = $true
        }
    }
}
if($haveMissing)
{
    Write-Host "You have settings file(s) that are not referenced in unit tests. Please see output above." -ForegroundColor Red
}
else {
    Write-Host "All settings file are referenced in unit tests." -ForegroundColor Green
}
