Get-ChildItem . -Include TestResults -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem . -Include TestStore -Recurse -Force | Remove-Item -Recurse -Force
