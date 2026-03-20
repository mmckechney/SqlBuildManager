Get-ChildItem ../../src -Include TestResults -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem ../../src -Include TestStore -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem ../tests/testresults | Remove-Item -Recurse -Force
