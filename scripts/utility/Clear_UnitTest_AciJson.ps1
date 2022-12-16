Write-Host "Deleting ACI unit test json files" -ForegroundColor Green

Get-ChildItem ../../src/SqlBuildManager.Console.ExternalTest/bin/Debug/net7.0 -Include aci-*.json -Recurse -Force | Remove-Item -Recurse -Force