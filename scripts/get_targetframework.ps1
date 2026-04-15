<#
.SYNOPSIS
    Reads and returns the target framework from the sbm.csproj project file.
.DESCRIPTION
    Parses the TargetFramework element from sbm.csproj (or a custom project path)
    and returns the value (e.g. "net10.0"). Used by build and packaging scripts
    to determine the correct output paths.
.PARAMETER path
    Optional path to the project directory containing sbm.csproj.
    Defaults to src\SqlBuildManager.Console.
#>
[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $path
)

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot "src\SqlBuildManager.Console"
}

$projFile = Join-Path $path "sbm.csproj"
$frameworkTarget = (Select-Xml -Path $projFile -XPath "/Project/PropertyGroup/TargetFramework").Node.InnerText
Write-Host $frameworkTarget

return $frameworkTarget