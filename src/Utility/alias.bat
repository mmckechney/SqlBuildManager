@echo off

DOSKEY ls=DIR $* 
DOSKEY cp=COPY $* 
DOSKEY xcp=XCOPY $*
DOSKEY mv=MOVE $* 
DOSKEY clear=CLS
DOSKEY sbm="%USERPROFILE%\source\repos\SqlBuildManager\bin\Debug\SqlBuildManager.Console.exe" $*