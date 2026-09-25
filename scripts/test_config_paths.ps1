function Get-TestConfigPath {
    param(
        [string] $envName,
        [string] $path,
        [string] $repoRoot = (Split-Path $PSScriptRoot -Parent),
        [switch] $Create
    )

    if ([string]::IsNullOrWhiteSpace($path)) {
        if ($envName -cnotmatch '^[a-z0-9-]{3,10}$') {
            throw 'Specify an azd environment name (3-10 lowercase letters, digits or hyphens), or an explicit -path directory.'
        }
        $path = Join-Path $repoRoot 'src' 'TestConfig' $envName
    }
    $path = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($path)
    if ($Create) {
        New-Item -ItemType Directory -Path $path -Force -ErrorAction Stop | Out-Null
    }
    if (-not (Test-Path -LiteralPath $path -PathType Container)) {
        throw "Test configuration directory '$path' does not exist. Generate configuration for '$envName' first; legacy root files are not used as a fallback."
    }
    return $path
}
