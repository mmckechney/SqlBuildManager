SET STARTUP_LOG=C:\startup_log.txt
ECHO Starting SqlBuildManager Services >> C:\startup_log.txt
SET LOCAL_APP_ROOT=%ROLEROOT%\AppRoot
ECHO %LOCAL_APP_ROOT% >> C:\startup_log.txt
ECHO %LOCAL_APP_ROOT%\bin\Azure >> C:\startup_log.txt

cd %LOCAL_APP_ROOT%\bin\Azure

ECHO Setting privilidges to the local resource folder  >> C:\startup_log.txt
powershell -ExecutionPolicy Unrestricted .\Set-Privs.ps1 >> C:\startup_log.txt

EXIT /B 0
