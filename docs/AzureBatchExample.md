# Azure Batch execution example
Below is an end-to-end example of running an Azure Batch build. This assumes that you have already created your batch account and uploaded your code package. If you have not, please refer follow the instructions found [here](./AzureBatch.md)

## 1. Save your batch settings to a `--settingsfile`
This only needs to be run once and this file can be reused for future builds as long as you don't change any keys or passwords. By creating this file now, you greatly simplify the subsequent commands. 

Replace the sample values with your own account values and keys

```bash
 sbm.exe batch savesettings ^
    --settingsfile "C:\temp\my_settings.json" ^
    --settingsfilekey [ "C:\opt\path_to_key_file.txt" or "key string"] ^
    --username myusernameId ^
    --password SuperSecurePassword ^
    --batchnodecount 2 ^
    --batchvmsize STANDARD_DS1_V2 ^
    --batchaccountname mybatchaccount ^
    --batchaccounturl "https://mybatchaccount.eastus.batch.azure.com" ^
    --batchaccountkey "Nx0scfUnl5lu-T5dEyPW2AwjYZSCBJvLzty+xPdsK9xPK2VCS4jl6fcdZSiqrM2F15Z214Jj5ajgl7RVAH9HqQ==" ^
    --storageaccountname batchstorage123 ^
    --storageaccountkey "1VT2/+3Xh4XzK6vSYCHHacbeRJth2/gEoy+buaGlq+oXvhQ19NQG9/D8sSgSCJ1Z+ICB/GrxJMvCI+xnaM5cQg==" ^
    --deletebatchpool False ^
    --deletebatchjob False ^
    --rootloggingpath "C:\temp" ^
    --eventhubconnection "Endpoint=sb://myeventhub.servicebus.windows.net/;SharedAccessKeyName=keyname;SharedAccessKey=KPnb2SyLfQz5jY1LqXl3TxnMBuJJn4id6OCJ7n4yYEo=;EntityPath=hubname"
```

**IMPORTANT** If you want to customize the concurrency of the script execution, you can also add `--concurrency` and `--concurrencytype` values. See [Concurrency Options](Concurrency_Options.md) to understand their usage 

## 2. Pre-stage your Batch VM nodes
This will create the VMs and get them ready to accept a build. Doing this in advance of your actual build time will eliminate the VM creation time from your overall build. It can take 10-30 minutes depending on your VM request

```bash
sbm.exe batch prestage --settingsfile "C:\temp\my_settings.json" --settingsfilekey [ "C:\opt\path_to_key_file.txt" or "key string"]
```

## 3. Run your Build (schema changes)
This command will leverage a DACPAC execution. It will create the source DACPAC from the `--platinumdbsource` database, create a DACPAC for the target `--database` then create the diff scripts SBM package and run this against all the targets defined in the `--override` config file.

If a database throws an error and rolls back the build, the process will create an individual DACPAC for that database and create a custom update package to ensure that in the end it is in sync with the `--platinumdbsource`

```bash
sbm.exe batch run ^
    --settingsfile "C:\temp\my_settings.json" ^
    --settingsfilekey [ "C:\opt\path_to_key_file.txt" or "key string"] ^
    --override azuredb.cfg ^
    --platinumdbsource "MyPlatinumDb" ^
    --platinumserversource "platinumserver.database.windows.net" ^
    --database MyTargetDb ^
    --server "targetserver.database.windows.net" ^
    --batchjobname "Release 27 schema"
```

## 4. Run your build (data updates)
This command will run data updates - for example fact/lookup table values or data cleanup. This assumes that you have packaged these scripts in a SBM file via one of the various means available to you .

```bash
sbm.exe batch run ^
    --settingsfile "C:\temp\my_settings.json" ^
    --settingsfilekey [ "C:\opt\path_to_key_file.txt" or "key string"] ^
    --override azuredb.cfg ^
    --packagename "myBuildPackage.sbm" ^
    --batchjobname "Release 27 data"
```

## 5. Clean up your batch nodes
Once you have completed your build via Azure Batch, you should delete your compute nodes so that you are no longer getting charged for them

```bash
sbm.exe batch cleanup --settingsfile "C:\temp\my_settings.json"  --settingsfilekey [ "C:\opt\path_to_key_file.txt" or "key string"] 
```