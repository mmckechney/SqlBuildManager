
# Azure Batch Command Reference

## Azure Batch Save Settings

`sbm batch savesettings [options]`\
This utility action will save a reusable JSON file to make running the command line easier for Batch processing.

The next time you run a build action, use the `--settingsfile="<file path>"` in place of the arguments below.

Can also optionally provide a `--settingsfilekey` value to provide a custom encryption key for encryption of the sensitive values (listed below). The value for the `--settingsfilekey`  can be either the encryption key itself of a path to a text file containing the key.

- Authentication: `--username`, `--password`
- Azure Batch: `--batchnodecount`, `--batchaccountname`, `--batchaccountkey`, `--batchaccounturl`, `--storageaccountname`, `--storageaccountkey`, `--batchvmsize`, `--deletebatchpool`, `--deletebatchjob`, `--pollbatchpoolstatus`, `--eventhubconnectionstring`
- Run time settings: `--rootloggingpath`, `--logastext`, `--concurrency`, `--concurrencytype`

_Note:_

1. the values for `--username`, `--password`, `--batchaccountkey`, `--storageaccountkey` and  `--eventhubconnectionstring` will be encrypted. Use a `--settingsfilekey` value to manage encryption
2. If there are duplicate values in the `--settingsfile` and the command line, the command line argument will take precedence.
3. You can hand-craft the JSON yourself in the [format below](#settings-file-format) but the password and keys will not be encrypted (which may be OK depending on where you save the files)

## Pre-Stage Batch nodes

`sbm batch prestage [options]`\
_Note:_ You can also leverage the [`--settingsfile`](#azure-batch-save-settings) and `--settingsfilekey` options to reuse most of the arguments

- `--batchnodecount="##"` - Number of nodes to provision to run the batch job  (default is 10)
- `--batchvmsize="<size>"` - Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general)
- `--batchaccountname="<batch acct name>"` - String name of the Azure Batch account
- `--batchaccountkey="<batch acct key>"` - Account Key for the Azure Batch account
- `--batchaccounturl="<batch acct url>"` - URL for the Azure Batch account
- `--pollbatchpoolstatus=(true|false)` - Whether or not you want to get updated status (true, default) or fire and forget (false)

## Batch Execution

`sbm batch run [options]`\
In addition to the [authentication](commandline.md#General-Authentication-settings) and [runtime](commandline.md#General-Runtime-settings) arguments above, these are specifically needed for Azure Batch executions.
\
_Note:_

1. You can also leverage the [--settingsfile](#azure-batch-save-settings) and `--settingsfilekey` options to reuse most of the arguments
2. either `--platinumdacpac` _or_ `--packagename` are required. If both are given, then `--packagename` will be used.

- `--platinumdacpac="<filename>"` - Name of the dacpac containing the platinum schema
- `--packagename="<filename>"` - Name of the .sbm or .sbx file to execute save execution logs
- `--batchaccountname="<batch acct name>"` - String name of the Azure Batch account
- `--batchaccountkey="<batch acct key>"` - Account Key for the Azure Batch account
- `--batchaccounturl="<batch acct url>"` - URL for the Azure Batch account
- `--storageaccountname="<storage acct name>"` - Name of storage account associated with the Azure Batch account  
- `--storageaccountkey="<storage acct key>"` - Account Key for the storage account
- `--batchjobname="<name>"` - [Optional] User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed

### Additional arguments

If you don't run the `sbm batch prestage`  and `sbm batch cleanup [options]` command sequence you will need to use the following:

- `--deletebatchpool=(true|false)` - Whether or not to delete the batch pool servers after an execution (default is `false`)
- `--deletebatchjob=(true|false)` - Whether or not to delete the batch job after an execution (default is `true`)
- `--batchnodecount="##"` - Number of nodes to provision to run the batch job  (default is 10)
- `--batchvmsize="<size>"` - Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general)

## Batch Clean Up Batch Nodes

`sbm batch cleanup [options]`\
_Note:_ You can also leverage the [--settingsfile](#azure-batch-save-settings) and `--settingsfilekey` options to reuse most of the arguments

- `--batchaccountname="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `--batchaccountkey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `--batchaccounturl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `--pollbatchpoolstatus=(true|false)` - Whether or not you want to get updated status (true, default) or fire and forget (false)

## Settings File Format

The format for the saved settings JSON file is below. You can include or exclude any values that would like. Also as a reminder, for any duplicate keys found in the settings file and command line arguments, the command line argument's value will be used.

```json
{
  "AuthenticationArgs": {
    "UserName": "<database use name>",
    "Password": "<database password>"
  },
  "BatchArgs": {
    "BatchNodeCount": "<int value>",
    "BatchAccountName": "<batch account name>",
    "BatchAccountKey": "<key for batch account ",
    "BatchAccountUrl": "<https URL for batch account>",
    "StorageAccountName": "<storage account name>",
    "StorageAccountKey": "<storage account key>",
    "BatchVmSize": "<VM size designator>",
    "DeleteBatchPool": "<true|false>",
    "DeleteBatchJob": "<true|false>",
    "PollBatchPoolStatus": "<true|false>",
    "EventHubConnectionString": "<connection string to EventHub (optional)>",
    "BatchPoolName": "[SqlBuildManagerPoolWindows or SqlBuildManagerPoolLinux]",
    "BatchPoolOs": "[Windows or Linux]",
    "ApplicationPackage": "<name of the application package to use>",
    "ServiceBusTopicConnectionString" : "<connection string to Service Bus Topic (optional)>"
  },
  "RootLoggingPath": "<valid folder path>",
  "DefaultScriptTimeout" : "<int>",
  "TimeoutRetryCount" : "<int>",
  "Concurrency": "<int value>",
  "ConcurrencyType": "[Count | Server | MaxPerServer]"
}
```
