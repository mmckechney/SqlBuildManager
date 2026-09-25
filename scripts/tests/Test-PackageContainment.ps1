<#
.SYNOPSIS
    Local SEC-07 integration tests against the real sbm CLI.
.DESCRIPTION
    Requires PowerShell 7 and a built sbm executable (or sbm.dll). No Azure, database,
    credentials, or Docker are used. All fixtures and child-process logs live in an
    isolated temporary directory, removed on completion. No package budgets are imposed.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SbmPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$SbmPath = (Resolve-Path -LiteralPath $SbmPath).Path
$root = Join-Path ([IO.Path]::GetTempPath()) ("sbm-cli-containment-" + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($root)
$failures = [Collections.Generic.List[string]]::new()
$checks = 0

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Invoke-Sbm([string[]] $CliArgs, [bool] $Success) {
    $info = [Diagnostics.ProcessStartInfo]::new()
    $info.FileName = if ([IO.Path]::GetExtension($SbmPath) -eq '.dll') { 'dotnet' } else { $SbmPath }
    if ([IO.Path]::GetExtension($SbmPath) -eq '.dll') { $info.ArgumentList.Add($SbmPath) }
    foreach ($arg in $CliArgs) { $info.ArgumentList.Add($arg) }
    $info.WorkingDirectory = $root
    $info.UseShellExecute = $false
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    # Keep application-owned extraction directories and logs inside this test's workspace.
    $info.Environment['TEMP'] = $root
    $info.Environment['TMP'] = $root
    $info.Environment['TMPDIR'] = $root
    $process = [Diagnostics.Process]::Start($info)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(60000)) {
            $process.Kill($true)
            $process.WaitForExit()
            throw "CLI integration test timed out: $($CliArgs -join ' ')"
        }
        $text = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
        Assert-True (($process.ExitCode -eq 0) -eq $Success) "Unexpected exit $($process.ExitCode): $($CliArgs -join ' ')`n$text"
        Assert-True (-not $text.Contains('OUTSIDE_SECRET_SENTINEL')) "CLI disclosed outside-file contents."
        return $text
    }
    finally { $process.Dispose() }
}

function New-Fixture([string] $Name, [string] $Reference, [string[]] $Entries, [switch] $LinkEntry) {
    $path = Join-Path $root "$Name.sbm"
    $escaped = [Security.SecurityElement]::Escape($Reference)
    $xml = @"
<SqlSyncBuildData xmlns="http://schemas.mckechney.com/SqlSyncBuildProject.xsd">
  <SqlSyncBuildProject ProjectName="Containment" ScriptTagRequired="false">
    <Scripts><Script FileName="$escaped" BuildOrder="1" ScriptId="test-script" Database="client" /></Scripts>
  </SqlSyncBuildProject>
</SqlSyncBuildData>
"@
    $archive = [IO.Compression.ZipFile]::Open($path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $writer = [IO.StreamWriter]::new($archive.CreateEntry('SqlSyncBuildProject.xml').Open())
        try { $writer.Write($xml) } finally { $writer.Dispose() }
        foreach ($name in $Entries) {
            $entry = $archive.CreateEntry($name)
            if ($LinkEntry) { $entry.ExternalAttributes = 0xA1FF -shl 16 }
            $writer = [IO.StreamWriter]::new($entry.Open())
            try { $writer.Write('SELECT 1;') } finally { $writer.Dispose() }
        }
    }
    finally { $archive.Dispose() }
    return $path
}

function Test-Case([string] $Name, [scriptblock] $Action) {
    $script:checks++
    try {
        & $Action
        Write-Host "PASS: $Name"
    }
    catch {
        $script:failures.Add("${Name}: $($_.Exception.Message)")
        Write-Host "FAIL: ${Name}: $($_.Exception.Message)"
    }
}

try {
    $source = Join-Path $root 'input.sql'
    [IO.File]::WriteAllText($source, 'SELECT 1;')
    $sentinel = Join-Path $root 'outside.sql'
    [IO.File]::WriteAllText($sentinel, 'OUTSIDE_SECRET_SENTINEL')
    $valid = Join-Path $root 'valid.sbm'

    Test-Case 'Create, list, hash, unpack and repackage a valid SBM' {
        $null = Invoke-Sbm @('create', 'fromscripts', '--outputsbm', $valid, '--scripts', $source) $true
        $listing = Invoke-Sbm @('list', '--packages', $valid, '--withhash') $true
        Assert-True ($listing.Contains('input.sql')) 'Valid script missing from CLI listing.'
        $hash = Invoke-Sbm @('gethash', '--packagename', $valid) $true
        Assert-True ($hash -match '(?m)^[A-Fa-f0-9]{40}\r?$') 'No package hash returned.'
        $destination = Join-Path $root 'roundtrip'
        $null = Invoke-Sbm @('unpack', '--package', $valid, '--directory', $destination) $true
        Assert-True ([IO.File]::ReadAllText((Join-Path $destination 'input.sql')) -eq 'SELECT 1;') 'Unpack changed script contents.'
        Assert-True ([IO.File]::Exists((Join-Path $destination 'valid.sbx'))) 'Unpack did not produce SBX.'
        $null = Invoke-Sbm @('package', '--directory', $destination) $true
        $repacked = Join-Path $destination 'valid.sbm'
        $repackedHash = Invoke-Sbm @('gethash', '--packagename', $repacked) $true
        $firstHash = [regex]::Match($hash, '(?m)^[A-Fa-f0-9]{40}\r?$').Value.Trim()
        Assert-True ($repackedHash.Contains($firstHash)) 'Roundtrip changed package hash.'
    }

    $fixtures = @(
        @{ Name = 'parent'; Reference = '../outside.sql'; Entries = @('a.sql') },
        @{ Name = 'windows-parent'; Reference = '..\outside.sql'; Entries = @('a.sql') },
        @{ Name = 'absolute'; Reference = $sentinel; Entries = @('a.sql') },
        @{ Name = 'missing'; Reference = 'leftover.sql'; Entries = @('a.sql') },
        @{ Name = 'collision'; Reference = 'a.sql'; Entries = @('one/a.sql', 'two/a.sql') },
        @{ Name = 'case-collision'; Reference = 'a.sql'; Entries = @('a.sql', 'A.sql') },
        @{ Name = 'archive-traversal'; Reference = 'a.sql'; Entries = @('a.sql', '../outside.sql') },
        @{ Name = 'link'; Reference = 'a.sql'; Entries = @('a.sql'); LinkEntry = $true }
    )
    foreach ($fixture in $fixtures) {
        $package = New-Fixture @fixture
        foreach ($command in @('unpack', 'list', 'gethash')) {
            Test-Case "$command rejects $($fixture.Name)" {
                $destination = Join-Path $root "$($fixture.Name)-$command"
                [void][IO.Directory]::CreateDirectory($destination)
                $leftover = Join-Path $destination 'leftover.sql'
                [IO.File]::WriteAllText($leftover, 'keep me')
                $cliArgs = switch ($command) {
                    'unpack' { @('unpack', '--package', $package, '--directory', $destination) }
                    'list' { @('list', '--packages', $package, '--withhash') }
                    'gethash' { @('gethash', '--packagename', $package) }
                }
                $null = Invoke-Sbm $cliArgs $false
                Assert-True ([IO.File]::ReadAllText($sentinel) -eq 'OUTSIDE_SECRET_SENTINEL') 'Outside sentinel was modified.'
                Assert-True ([IO.File]::ReadAllText($leftover) -eq 'keep me') 'Caller-owned destination was removed or modified.'
                Assert-True ([IO.Directory]::GetFiles($destination).Count -eq 1) 'Invalid package wrote files into the destination.'
            }
        }
    }

    Test-Case 'Unpack rejects a linked destination' {
        $target = Join-Path $root 'link-target'
        $link = Join-Path $root 'linked-output'
        [void][IO.Directory]::CreateDirectory($target)
        $keep = Join-Path $target 'keep.txt'
        [IO.File]::WriteAllText($keep, 'keep me')
        $null = New-Item -ItemType $(if ($IsWindows) { 'Junction' } else { 'SymbolicLink' }) -Path $link -Target $target
        try {
            $null = Invoke-Sbm @('unpack', '--package', $valid, '--directory', $link) $false
            Assert-True ([IO.File]::ReadAllText($keep) -eq 'keep me') 'Link target was modified.'
            Assert-True ([IO.Directory]::GetFiles($target).Count -eq 1) 'Extraction followed the directory link.'
        }
        finally { [IO.Directory]::Delete($link) }
    }

    if ($failures.Count -gt 0) {
        throw "$($failures.Count) of $checks CLI integration cases failed:`n$($failures -join "`n")"
    }
    Write-Host "PASS: $checks local CLI integration cases. No Azure or database calls."
}
finally {
    [IO.Directory]::Delete($root, $true)
}
