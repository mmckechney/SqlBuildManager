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

echo "Waiting for PostgreSQL to be ready..."
for i in $(seq 1 $RETRIES); do
    if timeout 2 bash -c "echo > /dev/tcp/localhost/5432" 2>/dev/null; then
        echo "PostgreSQL port is open, waiting for initialization to complete..."
        sleep 5
        echo "PostgreSQL should be ready."
        break
    fi
    echo "  Attempt $i/$RETRIES - PostgreSQL not ready yet..."
    sleep 5
done

mkdir -p /tests/TestResults

# Run test DLLs in order:
# 1. Pure unit tests (no external dependencies)
# 2. SQL Server dependent tests - SqlSync.SqlBuild first (creates databases)
# 3. PostgreSQL dependent tests
TEST_DLLS=(
    "SqlSync.SqlBuild.UnitTest/SqlSync.SqlBuild.UnitTest.dll"
    "SqlSync.ObjectScript.UnitTest/SqlSync.ObjectScript.UnitTest.dll"
    "SqlSync.Connection.UnitTest/SqlSync.Connection.UnitTest.dll"
    "SqlSync.DbInformation.UnitTest/SqlSync.DbInformation.UnitTest.dll"
    "SqlBuildManager.ScriptHandling.UnitTest/SqlBuildManager.ScriptHandling.UnitTest.dll"
    "SqlBuildManager.Console.UnitTest/SqlBuildManager.Console.UnitTest.dll"
    "SqlBuildManager.Enterprise.UnitTest/SqlBuildManager.Enterprise.UnitTest.dll"
    "SqlSync.SqlBuild.Dependent.UnitTest/SqlSync.SqlBuild.Dependent.UnitTest.dll"
    "SqlBuildManager.Console.Dependent.UnitTest/SqlBuildManager.Console.Dependent.UnitTest.dll"
    "SqlSync.ObjectScript.Dependent.UnitTest/SqlSync.ObjectScript.Dependent.UnitTest.dll"
    "SqlSync.DbInformation.Dependent.UnitTest/SqlSync.DbInformation.Dependent.UnitTest.dll"
    "SqlSync.Connection.Dependent.UnitTest/SqlSync.Connection.Dependent.UnitTest.dll"
    "SqlSync.SqlBuild.Dependent.PostgreSQL.UnitTest/SqlSync.SqlBuild.Dependent.PostgreSQL.UnitTest.dll"
    "SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest/SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest.dll"
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
