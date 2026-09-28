function Get-MySqlEntraClientArguments {
    param(
        [Parameter(Mandatory=$true)][string] $Server,
        [Parameter(Mandatory=$true)][string] $User,
        [Parameter(Mandatory=$true)][string] $ClientVersion
    )

    $arguments = @("--host=$Server", '--port=3306', "--user=$User", '--batch', '--skip-column-names')
    # Ubuntu's default-mysql-client is MariaDB and does not accept MySQL's --ssl-mode.
    if ($ClientVersion -match 'MariaDB') {
        $arguments += @('--ssl', '--ssl-verify-server-cert')
    }
    else {
        $arguments += @('--enable-cleartext-plugin', '--ssl-mode=VERIFY_IDENTITY')
    }
    if (Test-Path '/etc/ssl/certs/ca-certificates.crt') {
        $arguments += '--ssl-ca=/etc/ssl/certs/ca-certificates.crt'
    }
    return $arguments
}

function Assert-MySqlEntraPrincipal {
    param(
        [Parameter(Mandatory=$true)][string] $Mapping,
        [Parameter(Mandatory=$true)][guid] $ClientId,
        [Parameter(Mandatory=$true)][guid] $PrincipalId
    )

    $fields = $Mapping.Trim() -split "`t", 2
    $expectedId = "(?i)(?<![a-f0-9-])($ClientId|$PrincipalId)(?![a-f0-9-])"
    if ($fields.Count -ne 2 -or $fields[0] -ne 'aad_auth' -or $fields[1] -notmatch $expectedId) {
        throw 'The existing MySQL user is not mapped to the expected managed identity. No user was dropped or replaced. Review the conflicting principal before retrying.'
    }
}

function Invoke-MySqlEntraQuery {
    param(
        [Parameter(Mandatory=$true)][string[]] $Arguments,
        [Parameter(Mandatory=$true)][string] $Query,
        [Parameter(Mandatory=$true)][string] $Operation
    )

    for ($attempt = 1; $attempt -le 6; $attempt++) {
        $output = & mysql @Arguments "--execute=$Query" 2>&1
        $exitCode = $LASTEXITCODE
        $text = ($output | Out-String).Trim()
        if ($exitCode -eq 0) {
            return $text
        }
        if (-not [string]::IsNullOrEmpty($env:MYSQL_PWD)) {
            $text = $text.Replace($env:MYSQL_PWD, '<redacted>')
        }
        if ($attempt -lt 6 -and $text -match 'ERROR (9127|1045)\b') {
            Write-Warning "$Operation is waiting for Entra administrator/directory permission propagation (attempt $attempt of 6)."
            Start-Sleep -Seconds 10
            continue
        }
        throw "$Operation failed: $text. Check the Entra administrator, server identity Graph permissions and bootstrap token expiry; rerun postprovision to obtain a fresh token."
    }
}
