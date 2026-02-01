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