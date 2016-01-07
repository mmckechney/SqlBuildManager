
set adalsqlinstallfile=adalsql.msi

"%~dp0%adalsqlinstallfile%" /q  /log c:\startuplog.log

:end
echo install-ad.cmd completed: %date:~-4,4%%date:~-10,2%%date:~-7,2%-%timehour: =0%%time:~3,2%

EXIT /B 0
