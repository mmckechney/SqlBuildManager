#!/bin/bash

#used in Azure Container Instance image to run tests that depend on SQL Server being available.
set -e

# Local Compose runs one database platform at a time.  The default remains
# "all" for the existing ACI workflow.
PLATFORM="${SBM_TEST_PLATFORM:-all}"
case "$PLATFORM" in
    sqlserver|postgresql|mysql|all) ;;
    *) echo "Unsupported SBM_TEST_PLATFORM: $PLATFORM"; exit 2 ;;
esac

RETRIES=30
if [ "$PLATFORM" = "sqlserver" ] || [ "$PLATFORM" = "all" ]; then for i in $(seq 1 $RETRIES); do
    echo "Waiting for SQL Server to be ready..."
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
fi

if [ "$PLATFORM" = "postgresql" ] || [ "$PLATFORM" = "all" ]; then for i in $(seq 1 $RETRIES); do
    echo "Waiting for PostgreSQL to be ready..."
    if timeout 2 bash -c "echo > /dev/tcp/localhost/5432" 2>/dev/null; then
        echo "PostgreSQL port is open, waiting for initialization to complete..."
        sleep 5
        echo "PostgreSQL should be ready."
        break
    fi
    echo "  Attempt $i/$RETRIES - PostgreSQL not ready yet..."
    sleep 5
done
fi

if [ "$PLATFORM" = "mysql" ] || [ "$PLATFORM" = "all" ]; then for i in $(seq 1 $RETRIES); do
    echo "Waiting for MySQL to be ready..."
    if timeout 2 bash -c "echo > /dev/tcp/localhost/3306" 2>/dev/null; then
        echo "MySQL port is open, waiting for initialization to complete..."
        sleep 10
        echo "MySQL should be ready."
        break
    fi

    echo "  Attempt $i/$RETRIES - MySQL not ready yet..."
    sleep 5
done
fi

if [ -n "${SBM_TEST_EVENTHUB_CONNECTION_STRING:-}" ]; then
    echo "Waiting for local messaging emulators..."
    for hostport in "azurite:10000" "eventhubs-emulator:5672" "servicebus-emulator:5672"; do
        host="${hostport%%:*}"; port="${hostport##*:}"
        for i in $(seq 1 $RETRIES); do
            if timeout 2 bash -c "echo > /dev/tcp/$host/$port" 2>/dev/null; then break; fi
            echo "  Attempt $i/$RETRIES - $hostport not ready yet..."
            sleep 5
        done
    done
fi

mkdir -p /tests/TestResults

# Local Compose selects a small subset of the existing dependent suites.
case "$PLATFORM" in
    sqlserver) TEST_DLLS=(
        "SqlBuildManager.LocalContainer.SqlServer.IntegrationTest/SqlBuildManager.LocalContainer.SqlServer.IntegrationTest.dll"
        "SqlBuildManager.SqlBuild.SqlServer.IntegrationTest/SqlBuildManager.SqlBuild.SqlServer.IntegrationTest.dll"
        "SqlBuildManager.Console.SqlServer.IntegrationTest/SqlBuildManager.Console.SqlServer.IntegrationTest.dll"
    ) ;;
    postgresql) TEST_DLLS=(
        "SqlBuildManager.LocalContainer.PostgreSQL.IntegrationTest/SqlBuildManager.LocalContainer.PostgreSQL.IntegrationTest.dll"
        "SqlBuildManager.SqlBuild.PostgreSQL.IntegrationTest/SqlBuildManager.SqlBuild.PostgreSQL.IntegrationTest.dll"
        "SqlBuildManager.Console.PostgreSQL.IntegrationTest/SqlBuildManager.Console.PostgreSQL.IntegrationTest.dll"
    ) ;;
    mysql) TEST_DLLS=(
        "SqlBuildManager.LocalContainer.MySql.IntegrationTest/SqlBuildManager.LocalContainer.MySql.IntegrationTest.dll"
        "SqlBuildManager.SqlBuild.MySQL.IntegrationTest/SqlBuildManager.SqlBuild.MySQL.IntegrationTest.dll"
        "SqlBuildManager.Console.MySQL.IntegrationTest/SqlBuildManager.Console.MySQL.IntegrationTest.dll"
    ) ;;
    all) ;;
esac

# Run test DLLs in order:
# 1. Pure unit tests (no external dependencies)
# 2. SQL Server dependent tests - SqlBuildManager.SqlBuild first (creates databases)
# 3. PostgreSQL and MySQL dependent tests
if [ "$PLATFORM" = "all" ]; then TEST_DLLS=(
    "SqlBuildManager.SqlBuild.UnitTest/SqlBuildManager.SqlBuild.UnitTest.dll"
    "SqlBuildManager.ObjectScript.UnitTest/SqlBuildManager.ObjectScript.UnitTest.dll"
    "SqlBuildManager.Connection.UnitTest/SqlBuildManager.Connection.UnitTest.dll"
    "SqlBuildManager.DbInformation.UnitTest/SqlBuildManager.DbInformation.UnitTest.dll"
    "SqlBuildManager.ScriptHandling.UnitTest/SqlBuildManager.ScriptHandling.UnitTest.dll"
    "SqlBuildManager.Console.UnitTest/SqlBuildManager.Console.UnitTest.dll"
    "SqlBuildManager.Enterprise.UnitTest/SqlBuildManager.Enterprise.UnitTest.dll"
    "SqlBuildManager.SqlBuild.SqlServer.IntegrationTest/SqlBuildManager.SqlBuild.SqlServer.IntegrationTest.dll"
    "SqlBuildManager.Console.SqlServer.IntegrationTest/SqlBuildManager.Console.SqlServer.IntegrationTest.dll"
    "SqlBuildManager.ObjectScript.IntegrationTest/SqlBuildManager.ObjectScript.IntegrationTest.dll"
    "SqlBuildManager.DbInformation.IntegrationTest/SqlBuildManager.DbInformation.IntegrationTest.dll"
    "SqlBuildManager.Connection.IntegrationTest/SqlBuildManager.Connection.IntegrationTest.dll"
    "SqlBuildManager.SqlBuild.PostgreSQL.IntegrationTest/SqlBuildManager.SqlBuild.PostgreSQL.IntegrationTest.dll"
    "SqlBuildManager.Console.PostgreSQL.IntegrationTest/SqlBuildManager.Console.PostgreSQL.IntegrationTest.dll"
    "SqlBuildManager.SqlBuild.MySQL.IntegrationTest/SqlBuildManager.SqlBuild.MySQL.IntegrationTest.dll"
    "SqlBuildManager.Console.MySQL.IntegrationTest/SqlBuildManager.Console.MySQL.IntegrationTest.dll"
); fi

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
