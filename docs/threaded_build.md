# Threaded Build Execution

If you have a handful of databases, you can run them in parallel with a threaded build. You can control the concurrency of the tasks - refer to the [concurrency options](concurrency_options.md) for full details.

To manage the database targets, you will need an override target file. The details of the contents of this file can be found [here](override_options.md)

Sample command:

``` bash
sbm threaded run
    --packagename "mypackage.sbm" ^
    --username "dbusername" ^
    --password "dbpassword" ^
    --override "targetdatabases.cfg" ^

```