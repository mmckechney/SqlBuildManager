# Azure Batch execution example
Below is an end-to-end example of running an Azure Batch build. This assumes that you have already created your batch account and uploaded your code package. If you have not, please refer follow the instructions found [here](./azure_batch.md)

## 0. Connect to your Azure Account

``` bash 
az login
```

## 1. Save your batch settings to a `--settingsfile`
This only needs to be run once and this file can be reused for future builds as long as you don't change any keys or passwords. By creating this file now, you greatly simplify the subsequent commands. If you include the `--keyvault` option, the secrets will be saved in Azure Key vault and not the settings file. You can also simplify the collection of the various keys and connection strings and saving by using the PowerShell script [create_batch_settingsfiles.ps1](../scripts/templates/create_batch_settingsfiles.ps1)

Replace the sample values with your own account values and keys

```bash
 sbm batch savesettings ^
    --settingsfile "C:\temp\my_settings.json" ^
    --settingsfilekey [ "C:\opt\path_to_key_file.txt" or "key string"] ^ # or --keyvault "<key vault name>"
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
    --servicebustopicconnection "Endpoint=sb://myservicebus.servicebus.windows.net/;SharedAccessKeyName=sbmtopicpolicy;SharedAccessKey=ik14RTc6nhVepUr77V51sM4p48xdE/IcGtsfe5FolDA=;EntityPath=sqlbuildmanager
```

## 2. Pre-stage your Batch VM nodes

This will create the VMs and get them ready to accept a build. Doing this in advance of your actual build time will eliminate the VM creation time from your overall build. It can take 10-30 minutes depending on your VM request

``` bash
sbm batch prestage --settingsfile "C:\temp\my_settings.json" --settingsfilekey [ "C:\opt\path_to_key_file.txt" or "key string"]

# or

sbm batch prestage --settingsfile "C:\temp\my_settings.json" --keyvaultname "<key vault name>"
```

## 3. Queue your database targets

**IMPORTANT** Make sure you understand the implications of the `--concurrency` and `--concurrencytype` value combinations. See [Concurrency Options](concurrency_options.md) to understand their usage

This will send the database targets to the Service Bus Topic named `sqlbuildmanager`. The topic will be populated with one message per topic. The `--concurrencytype` value is used to determine the Service Bus subscription to populate. The `--batchjobname` is used to label each message. Both the `--concurrencytype` and `--batchjobname` values must be the same when running the build, otherwise it will not run properly.

``` bash
sbm batch enqueue ^
    --settingsfile "C:\temp\my_settings.json" ^
    --settingsfilekey "C:\keys\my-settings-key.txt" ^ # or --keyvault "<key vault name>"
    --override "C:\temp\overrides.cfg" ^
    --concurrencytype MaxPerServer ^
    --batchjobname "Release 27 schema"
```

## 4. Run your Build (schema changes)

This command will leverage a DACPAC execution. It will create the diff scripts in an SBM package and run this against all the targets that have been previously queued.\
It will know to use Service Bus because the `ServiceBusTopicConnectionString` property is set in the `--settingsfile`. The `--override` setting is only used to generate the diff scripts

If a database throws an error and rolls back the build, the process will create an individual DACPAC for that database and create a custom update package to ensure that in the end it is in sync with the `--platinumdbsource`

```bash
sbm.exe batch run ^
    --settingsfile "C:\temp\my_settings.json" ^
    --settingsfilekey "C:\keys\my-settings-key.txt" ^ # or --keyvault "<key vault name>"
    --platinumdacpac "C:\temp\myplatdb.dacpac" ^
    --override "C:\temp\overrides.cfg"
    --batchjobname "Release 27 schema" ^
    --concurrencytype MaxPerServer ^
    --concurrency 2
```

## 4. Re-Queue your database targets for data updates

If you have data updates to run. Re-populate the Service Bus Topic with your override targets

``` bash
sbm batch enqueue ^
    --settingsfile "C:\temp\my_settings.json" ^ 
    --settingsfilekey "C:\keys\my-settings-key.txt" ^ # or --keyvault "<key vault name>"
    --override "C:\temp\overrides.cfg" ^
    --concurrencytype MaxPerServer ^
    --batchjobname "Release 27 data" 
```

## 5. Run your build (data updates)

This command will run data updates - for example fact/lookup table values or data cleanup. This assumes that you have packaged these scripts in a SBM file via one of the various means available to you. Notice that no `--override` value is required here, since there is no script generation and the database targets are in the Service Bus Topic

```bash
sbm.exe batch run ^
    --settingsfile "C:\temp\my_settings.json" ^
    --settingsfilekey "C:\keys\my-settings-key.txt" ^ # or --keyvault "<key vault name>"
    --packagename "myBuildPackage.sbm" ^
    --batchjobname "Release 27 data" ^
    --concurrencytype MaxPerServer ^
    --concurrency 2 ^
    --batchjobname "Release 27 data" 
```

## 6. Clean up your batch nodes

Once you have completed your build via Azure Batch, you should delete your compute nodes so that you are no longer getting charged for them

```bash
sbm.exe batch cleanup ^
    --settingsfile "C:\temp\my_settings.json"  ^
    --settingsfilekey "C:\keys\my-settings-key.txt" # or --keyvault "<key vault name>"
```
