param
(
    [string] $path = "..\..\src\TestConfig",
    [string] $testPath = "..\..\src\SqlBuildManager.Console.ExternalTest\"
)

$testSettingsFiles = Get-ChildItem $path
$haveMissing = $false
foreach($file in $testSettingsFiles)
{
    $name = $file.Name
    if($name.EndsWith("yaml") -or $name.EndsWith("json"))
    {
        Write-Host "Looking for refrences to: $name" -ForegroundColor Cyan
        $results = Get-ChildItem $testPath  -File | Select-String -pattern $name
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
