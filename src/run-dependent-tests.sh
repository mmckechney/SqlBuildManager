#!/bin/bash

#used in Azure Container Instance image to run tests that depend on SQL Server being available.
set -e

echo "Waiting for SQL Server to be ready..."
RETRIES=30
for i in $(seq 1 $RETRIES); do
    # TCP check on SQL Server port
    if timeout 2 bash -c "echo > /dev/tcp/localhost/1433" 2>/dev/null; then
        echo "SQL Server port is open, waiting for initialization to complete..."
        sleep 10
        echo "SQL Server should be ready."
        break
    fi
    echo "  Attempt $i/$RETRIES - SQL Server not ready yet..."
    sleep 5
done

mkdir -p /tests/TestResults

# Run test DLLs in order - SqlSync.SqlBuild first (creates databases)
TEST_DLLS=(
    "SqlSync.SqlBuild.Dependent.UnitTest/SqlSync.SqlBuild.Dependent.UnitTest.dll"
    "SqlBuildManager.Console.Dependent.UnitTest/SqlBuildManager.Console.Dependent.UnitTest.dll"
    "SqlSync.ObjectScript.Dependent.UnitTest/SqlSync.ObjectScript.Dependent.UnitTest.dll"
    "SqlSync.DbInformation.Dependent.UnitTest/SqlSync.DbInformation.Dependent.UnitTest.dll"
    "SqlSync.Connection.Dependent.UnitTest/SqlSync.Connection.Dependent.UnitTest.dll"
)

OVERALL_EXIT=0
for dll in "${TEST_DLLS[@]}"; do
    echo ""
    echo "============================================"
    echo "Running: $dll"
    echo "============================================"
    
    TEST_NAME=$(basename $(dirname "$dll"))
    
    if [ -n "$TEST_FILTER" ]; then
        dotnet vstest "/tests/$dll" \
            "--logger:trx;LogFileName=${TEST_NAME}.trx" \
            "--logger:html;LogFileName=${TEST_NAME}.html" \
            "--logger:console;verbosity=detailed" \
            "--TestCaseFilter:$TEST_FILTER" \
            --ResultsDirectory:/tests/TestResults 2>&1 | tee -a /tests/TestResults/console-output.log
    else
        dotnet vstest "/tests/$dll" \
            "--logger:trx;LogFileName=${TEST_NAME}.trx" \
            "--logger:html;LogFileName=${TEST_NAME}.html" \
            "--logger:console;verbosity=detailed" \
            --ResultsDirectory:/tests/TestResults 2>&1 | tee -a /tests/TestResults/console-output.log
    fi
    
    TEST_EXIT=${PIPESTATUS[0]}
    if [ $TEST_EXIT -ne 0 ]; then
        OVERALL_EXIT=$TEST_EXIT
    fi
done

echo ""
echo "TEST_EXIT_CODE=$OVERALL_EXIT"
exit $OVERALL_EXIT
