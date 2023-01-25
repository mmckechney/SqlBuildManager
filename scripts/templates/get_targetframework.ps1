[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $path = "..\..\src\SqlBuildManager.Console"
)

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
$projFile = Join-Path $scriptDir $path "sbm.csproj"
$frameworkTarget = (Select-Xml -Path $projFile -XPath "/Project/PropertyGroup/TargetFramework").Node.InnerText
Write-Host $frameworkTarget

return $frameworkTarget