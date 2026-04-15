<#
.SYNOPSIS
    Runs all unit tests in the solution without rebuilding.
.DESCRIPTION
    Executes 'dotnet test --no-build' against the SQLSync.sln solution with detailed
    console output. Assumes the solution has already been built.
#>
dotnet test --no-build ../../src/SQLSync.sln -l "console;verbosity=detailed"