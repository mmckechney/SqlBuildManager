REM Set the value of netfx to install appropriate .NET Framework. 
REM ***** To install .NET 4.5.2 set the variable netfx to "NDP452" *****
REM ***** To install .NET 4.6 set the variable netfx to "NDP46" *****
REM ***** To install .NET 4.6.1 set the variable netfx to "NDP461" *****
set netfx="NDP461"

REM ***** Needed to correctly install .NET 4.6.1, otherwise you may see an out of disk space error *****
set TMP=C:\temp
set TEMP=C:\temp

REM ***** Setup .NET filenames and registry keys *****
if %netfx%=="NDP461" goto NDP461
if %netfx%=="NDP46" goto NDP46
    set netfxinstallfile="NDP452-KB2901954-Web.exe"
    set netfxregkey="0x5cbf5"
    goto install

:NDP46
set netfxinstallfile="NDP46-KB3045560-Web.exe"
set netfxregkey="0x60051"
goto install

:NDP461
set netfxinstallfile="NDP461-KB3102438-Web.exe"
set netfxregkey="0x6041f"

:install
REM ***** Check if .NET is installed *****
echo Checking if .NET (%netfx%) is installed 
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release | Find %netfxregkey%
if %ERRORLEVEL%== 0 goto preinstalled

REM ***** Installing .NET *****
echo Installing .NET: start /wait %~dp0%netfxinstallfile% /q /serialdownload 
start /wait %~dp0%netfxinstallfile% /q /serialdownload 

:end
echo install.cmd completed: %date:~-4,4%%date:~-10,2%%date:~-7,2%-%timehour: =0%%time:~3,2% 
EXIT /B 0

:preinstalled
echo .NET (%netfx%) is already installed!
EXIT /B 0
