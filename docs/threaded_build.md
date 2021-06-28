# Threaded Build Execution

If you have a handful of databases, you can run them in parallel with a threaded build (multiple update threads running from a single machine). You can control the concurrency of the tasks - refer to the [concurrency options](concurrency_options.md) for full details.

To manage the database targets, you will need an override target file. The details of the contents of this file can be found [here](override_options.md)

Sample command:

``` bash
sbm threaded run
    --packagename "mypackage.sbm" ^
    --username "dbusername" ^
    --password "dbpassword" ^
    --override "targetdatabases.cfg" ^
```

When you execute an `sbm threaded run` build, in addition to the console output, it will create a number of log files. They will be created in the location of the `--rootloggingpath` setting or in the current directory if `--rootloggingpath` is not provided. For details on the log files and their contents, please see [Log Files Details for Threaded and Batch execution](threaded_and_batch_logs.md)