# Local Builds

If you have a single database to update, you can use a local build.

Sample command:

``` bash
sbm build 
    --packagename "mypackage.sbm" ^
    --username "dbusername" ^
    --password "dbpassword" ^
    --server "targetserver" ^
    --database "targetdb" ^
```

With a local build, the build history and log are stored within the `.sbm` file. You can get access to the logs either by opening up the Windows app `sqlbuildmanager.exe`'s Logging -> Show Build Logs menu or opening the `.sbm` file with a Zip file handler (7-zip, Windows zip handler, etc.) and see the `.log` files.

FYI: an `.sbm` file is a zip file that contains:

- All of the script files
- The run metadata in the `SqlSyncBuildProject.xml` file
- A build history summary in the `SqlSyncBuildHistory.xml` file (only included if there is a build history)
- Detailed run logs in `LogFile-{date-time stamp}.log` files. These file contain the scripts and script output logs for each run and will contain detailed error messaging for any scripts that might have failed, so you can investigate and fix the scripts for a future run. These are also constructed as a run-able SQL scripts if you ever wanted to run them separately (only included if there are previous runs)
