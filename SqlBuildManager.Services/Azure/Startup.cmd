SET STARTUP_LOG=%ROLEROOT%\startup_log.txt
SET LOCAL_APP_ROOT=%ROLEROOT%\AppRoot

cd %LOCAL_APP_ROOT%\Azure

ECHO Setting privilidges to the local resource folder  >> %STARTUP_LOG%
powershell .\Set-Privs.ps1 >> %STARTUP_LOG%

EXIT /B 0
