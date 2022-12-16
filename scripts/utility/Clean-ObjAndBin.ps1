Get-ChildItem ../../src -Include bin -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem ../../src -Include obj -Recurse -Force | Remove-Item -Recurse -Force
Remove-Item ../../src/.vs -Recurse -Force 
