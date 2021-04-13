
try {

    #Create a dated sub folder for the test results
    $datedTestResultFolder = "./TestResults/" +( (Get-Date -Format "s") -replace ":", "-")
    if(-not (Test-Path $datedTestResultFolder))
    {
        New-Item -Path $datedTestResultFolder -ItemType Directory
    }

    write-host 'Finding all unit test projects'  -ForegroundColor Cyan
    #get all of the unittest projects and run the tests
    $list = Get-ChildItem -Path . -Recurse | Where-Object { $_.PSIsContainer -eq $false -and $_.Extension -eq '.csproj' -and $_.Name -match "UnitTest"} 
    foreach($proj in $list)
    {
        write-host "Running tests for" $proj  -ForegroundColor Cyan
        dotnet test $proj --results-directory:$datedTestResultFolder --collect:"Code Coverage" -l "console;verbosity=detailed"
    }
    write-host 'Tests Completed'  -ForegroundColor Green


    $recentCoverageFiles = Get-ChildItem -File -Filter *.coverage -Path $datedTestResultFolder -Name -Recurse #| Select-Object -First 1;
    $filename = 0
    foreach($coverageFile in $recentCoverageFiles)
    {
        $filename++
        C:\Users\mimcke\.nuget\packages\microsoft.codecoverage\16.7.0-preview-20200519-01\build\netstandard1.0\CodeCoverage\CodeCoverage.exe analyze  /output:$datedTestResultFolder\$filename.coveragexml  $datedTestResultFolder'\'$coverageFile
        write-host 'CoverageXML Generated'  -ForegroundColor Green
    }

    write-host 'Creating coverage report'  -ForegroundColor Green
    reportgenerator "-reports:$($datedTestResultFolder)\*.coveragexml" "-targetdir:$($datedTestResultFolder)\coveragereport"
    write-host 'CoverageReport Published'  -ForegroundColor Green

}
catch {

    write-host "Caught an exception:" -ForegroundColor Red
    write-host "Exception Type: $($_.Exception.GetType().FullName)" -ForegroundColor Red
    write-host "Exception Message: $($_.Exception.Message)" -ForegroundColor Red

}