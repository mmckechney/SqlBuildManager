<#
.SYNOPSIS
    Deletes generated ACI JSON definition files from ExternalTest build output.
.DESCRIPTION
    Removes aci-*.json files created during ACI integration test runs from the
    ExternalTest project's build output directory.
#>
Write-Host "Deleting ACI unit test json files" -ForegroundColor Green

$frameworkTarget = (Select-Xml -Path "../../src/SqlBuildManager.Console.ExternalTest/SqlBuildManager.Console.ExternalTest.csproj" -XPath "/Project/PropertyGroup/TargetFramework").Node.InnerText
Get-ChildItem ../../src/SqlBuildManager.Console.ExternalTest/bin/Debug/$frameworkTarget -Include aci-*.json -Recurse -Force | Remove-Item -Recurse -Force