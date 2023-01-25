Write-Host "Deleting Kubernetes unit test YAML files" -ForegroundColor Green

$frameworkTarget = (Select-Xml -Path "../../src/SqlBuildManager.Console.ExternalTest/SqlBuildManager.Console.ExternalTest.csproj" -XPath "/Project/PropertyGroup/TargetFramework").Node.InnerText
Get-ChildItem ../../src/SqlBuildManager.Console.ExternalTest/bin/Debug/$frameworkTarget -Include *.yaml -Recurse -Force | Remove-Item -Recurse -Force
