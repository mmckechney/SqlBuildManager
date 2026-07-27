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

echo "Waiting for MySQL to be ready..."
for i in $(seq 1 $RETRIES); do
    if timeout 2 bash -c "echo > /dev/tcp/localhost/3306" 2>/dev/null; then
        echo "MySQL port is open, waiting for initialization to complete..."
        sleep 10
        echo "MySQL should be ready."
        break
    fi
    echo "  Attempt $i/$RETRIES - MySQL not ready yet..."
    sleep 5
done

mkdir -p /tests/TestResults

# Run test DLLs in order:
# 1. Pure unit tests (no external dependencies)
# 2. SQL Server dependent tests - SqlBuildManager.SqlBuild first (creates databases)
# 3. PostgreSQL and MySQL dependent tests
TEST_DLLS=(
    "SqlBuildManager.SqlBuild.UnitTest/SqlBuildManager.SqlBuild.UnitTest.dll"
    "SqlBuildManager.ObjectScript.UnitTest/SqlBuildManager.ObjectScript.UnitTest.dll"
    "SqlBuildManager.Connection.UnitTest/SqlBuildManager.Connection.UnitTest.dll"
    "SqlBuildManager.DbInformation.UnitTest/SqlBuildManager.DbInformation.UnitTest.dll"
    "SqlBuildManager.ScriptHandling.UnitTest/SqlBuildManager.ScriptHandling.UnitTest.dll"
    "SqlBuildManager.Console.UnitTest/SqlBuildManager.Console.UnitTest.dll"
    "SqlBuildManager.Enterprise.UnitTest/SqlBuildManager.Enterprise.UnitTest.dll"
    "SqlBuildManager.SqlBuild.Dependent.SqlServer.UnitTest/SqlBuildManager.SqlBuild.Dependent.SqlServer.UnitTest.dll"
    "SqlBuildManager.Console.Dependent.SqlServer.UnitTest/SqlBuildManager.Console.Dependent.SqlServer.UnitTest.dll"
    "SqlBuildManager.ObjectScript.Dependent.UnitTest/SqlBuildManager.ObjectScript.Dependent.UnitTest.dll"
    "SqlBuildManager.DbInformation.Dependent.UnitTest/SqlBuildManager.DbInformation.Dependent.UnitTest.dll"
    "SqlBuildManager.Connection.Dependent.UnitTest/SqlBuildManager.Connection.Dependent.UnitTest.dll"
    "SqlBuildManager.SqlBuild.Dependent.PostgreSQL.UnitTest/SqlBuildManager.SqlBuild.Dependent.PostgreSQL.UnitTest.dll"
    "SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest/SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest.dll"
    "SqlBuildManager.SqlBuild.Dependent.MySQL.UnitTest/SqlBuildManager.SqlBuild.Dependent.MySQL.UnitTest.dll"
    "SqlBuildManager.Console.Dependent.MySQL.UnitTest/SqlBuildManager.Console.Dependent.MySQL.UnitTest.dll"
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
