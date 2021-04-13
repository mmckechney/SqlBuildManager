Get-ChildItem . -Include bin -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem . -Include obj -Recurse -Force | Remove-Item -Recurse -Force
