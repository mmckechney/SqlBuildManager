<#
.SYNOPSIS
    Removes TestResults and TestStore directories from the src and tests folders.
.DESCRIPTION
    Cleans up test output artifacts including TestResults folders under src,
    TestStore folders under src, and downloaded test results in scripts/tests/testresults.
#>
Get-ChildItem ../../src -Include TestResults -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem ../../src -Include TestStore -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem ../tests/testresults | Remove-Item -Recurse -Force
