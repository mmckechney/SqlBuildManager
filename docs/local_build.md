# Local Builds

If you have a single databases to update, you can use a local build. 

Sample command:

``` bash
sbm build 
    --packagename "mypackage.sbm" ^
    --username "dbusername" ^
    --password "dbpassword" ^
    --server "targetserver" ^
    --database "targetdb" ^
```