Write-Host "Deleting Kubernetes unit test YAML files" -ForegroundColor Green
Get-ChildItem ../../src/SqlBuildManager.Console.ExternalTest/bin/Debug/net7.0 -Include *.yaml -Recurse -Force | Remove-Item -Recurse -Force
