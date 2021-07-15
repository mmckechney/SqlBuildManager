# Log Files Details for Threaded,  Batch and Containers execution

Whether running a `threaded` or `batch` run, the tooling will create several log files for you reference, auditing and troubleshooting. 

- For a `threaded run` these logs will be located in the current directory or in the path designated by the `--rootloggingpath` argument
- For a `batch run` the logs will be stored in the Azure Storage account associated with the Batch account. You can view the logs in several ways, including the the [Azure portal](http://portal.azure.com), [Azure Batch Explorer](https://azure.github.io/BatchExplorer/) and [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/). 

----

## File Types
- [Output Config Files](#output-config-files)
- [High level log files](#high-level-log-files)
- [Detailed Log Files](#detailed-log-files)
- [Troubleshooting tips](#troubleshooting-tips)

_Note:_ For a `batch run` these files are consolidated logs from across all of your Batch nodes and2 individually per node in the "Task#" subfolders 

----

## Output Config Files

There will be one (or two, depending on the run success) .cfg files created - `successdatabases.cfg` and/or `failuredatabases.cfg`. These files contain lists of all databases that were either successfully updated or had a failure and were rolled back, respectively. The format is the same use in  an `--override` argument so it is easy to use this file a an override target source for any follow-up runs that may be necessary.

Example contents for `successdatabases.cfg` and `failuredatabases.cfg`

``` log
sqllab2-b.database.windows.net:SqlBuildTest,SqlBuildTest001
sqllab2-a.database.windows.net:SqlBuildTest,SqlBuildTest006
sqllab2-b.database.windows.net:SqlBuildTest,SqlBuildTest006
sqllab2-a.database.windows.net:SqlBuildTest,SqlBuildTest001
sqllab2-b.database.windows.net:SqlBuildTest,SqlBuildTest007
sqllab2-a.database.windows.net:SqlBuildTest,SqlBuildTest007
sqllab2-a.database.windows.net:SqlBuildTest,SqlBuildTest002
```

----

## High level log files

For a quick reference, three high-level log files are created: `commits.log`,   `errors.log`, and `SqlBuildManager.ThreadedExecution.log`. The `commits.log` and `errors.log` files contain the top level success (commits) and failures (error) logs, including: time stamp, run id (a unique value for a single database build), server name, database name, and final return code for the database run (Committed, Rolled Back, etc.).

_NOTE_The second column in each of the samples below is the unique "run id" for the build and can be correlated across all log files for a specific build run.

Sample contents for `commits.log`

``` log
[2021-06-25 12:01:26.722 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest001: Committed
[2021-06-25 12:01:26.722 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest006: Committed
[2021-06-25 12:01:26.722 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest006: Committed
[2021-06-25 12:01:26.722 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest001: Committed
[2021-06-25 12:01:28.373 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest007: Committed
[2021-06-25 12:01:28.397 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest007: Committed
[2021-06-25 12:01:28.413 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest002: Committed
[2021-06-25 12:01:28.432 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest002: Committed
[2021-06-25 12:01:29.754 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest008: Committed
```

Sample contents for `errors.log`

``` log
[2021-06-25 11:48:57.944 ] fcf97151e8de4fcf939a43891f2b817b  sqllab2-a.database.windows.net  SqlBuildTest001: Rolled Back
[2021-06-25 11:48:57.948 ] fcf97151e8de4fcf939a43891f2b817b  sqllab2-a.database.windows.net  SqlBuildTest009: Rolled Back
[2021-06-25 11:48:57.952 ] fcf97151e8de4fcf939a43891f2b817b  sqllab2-a.database.windows.net  SqlBuildTest003: Rolled Back
[2021-06-25 11:48:57.965 ] fcf97151e8de4fcf939a43891f2b817b  sqllab2-b.database.windows.net  SqlBuildTest001: Rolled Back
[2021-06-25 11:48:58.007 ] fcf97151e8de4fcf939a43891f2b817b  sqllab2-a.database.windows.net  SqlBuildTest007: Rolled Back
[2021-06-25 11:48:58.015 ] fcf97151e8de4fcf939a43891f2b817b  sqllab2-b.database.windows.net  SqlBuildTest005: Rolled Back
[2021-06-25 11:48:58.022 ] fcf97151e8de4fcf939a43891f2b817b  sqllab2-b.database.windows.net  SqlBuildTest003: Rolled Back
```

The `SqlBuildManager.ThreadedExecution.log` file contains some additional detail specific to the concurrency and timing of each thread (Queuing up thread, Starting up thread, Thread complete) as well as some summary information on the duration and number of targets for a build.

Sample contents for `SqlBuildManager.ThreadedExecution.log`

``` log
[2021-06-25 12:01:31.251 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest010: Starting up thread
[2021-06-25 12:01:31.257 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest004: Thread complete
[2021-06-25 12:01:31.257 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest005: Queuing up thread
[2021-06-25 12:01:31.257 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest005: Starting up thread
[2021-06-25 12:01:31.284 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest004: Thread complete
[2021-06-25 12:01:31.284 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest005: Queuing up thread
[2021-06-25 12:01:31.284 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest005: Starting up thread
[2021-06-25 12:01:33.532 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest010: Thread complete
[2021-06-25 12:01:33.535 ] f929636010584a68827634229c9f3b3e  sqllab2-a.database.windows.net  SqlBuildTest005: Thread complete
[2021-06-25 12:01:33.541 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest010: Thread complete
[2021-06-25 12:01:33.768 ] f929636010584a68827634229c9f3b3e  sqllab2-b.database.windows.net  SqlBuildTest005: Thread complete
[2021-06-25 12:01:33.768 ] f929636010584a68827634229c9f3b3e    : Ending threaded processing at 6/25/2021 4:01:33 PM
[2021-06-25 12:01:33.768 ] f929636010584a68827634229c9f3b3e    : Execution Duration: 00:00:09.6377429
[2021-06-25 12:01:33.768 ] f929636010584a68827634229c9f3b3e    : Total number of targets: 20
```

----

## Detailed Log Files

### SqlBuildManager.Console{date stamp}.log

This file contains the detailed console output. It is a capture of all of the logs that stream by in your command/console window during a build run.

### "Working" folder

This is the folder that contains the runtime files such as the DACPAC, SBM and distributed database configuration files. It will also contain a sub-folder for each database server target. The folder structure is:
 - `<server name>` folders - There is one folder per target SQL Server. Within each of these is a folder for each target database. 
    - `<database name>` folders - within these folders are three files
      - `LogFile-\<date,time\>.log` -  a detailed script by script run result
      - `SqlSyncBuildHistory.xml` - detailed log along with script meta-data (such as start/end times, file hash, status, user id)
      - `SqlSyncBuildProject.xml` - meta-data file for the script package run against the database

----

## Troubleshooting tips

If you have SQL errors in your execution, you will probably want to figure out what happened. Here is a suggested troubleshooting path:

1. Open up the `failuredatabases.cfg` file to see what databases had problems
2. Taking note of the server and database name, open the server folder then the database folder
3. Open the `logfile` in the database folder. This file should contain an error message that will guide your troubleshooting should you need to correct some scripts
4. Once you have determined the problem, use the `failuredatabases.cfg` file as your `--override` argument to run your updates again - hopefully successfully this time!
