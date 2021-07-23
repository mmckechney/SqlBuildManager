
# Azure Batch Command Reference

## Azure Batch Save Settings

`sbm batch savesettings [options]`\
This utility action will save a reusable JSON file to make running the command line easier for Batch processing.

The next time you run a build action, use the `--settingsfile="<file path>"` in place of the arguments below. If you provide a `--keyvaultname` parameter value, the secrets will be saved to the specified Azure Key Vault and _not_ saved to the settings file. If you use this, you first must connect to Azure via the `az login` CLI command.

Can also optionally provide a `--settingsfilekey` value to provide a custom encryption key for encryption of the sensitive values (listed below). The value for the `--settingsfilekey`  can be either the encryption key itself or the path to a text file containing the key.

- 
- Authentication: `--username`, `--password`
- Azure Batch: `--batchnodecount`, `--batchaccountname`, `--batchaccountkey`, `--batchaccounturl`, `--storageaccountname`, `--storageaccountkey`, `--batchvmsize`, `--deletebatchpool`, `--deletebatchjob`, `--pollbatchpoolstatus`, `--eventhubconnectionstring`, `--servicebustopicconnection`
- Run time settings: `--rootloggingpath`, `--logastext`, `--concurrency`, `--concurrencytype`

_Note:_

1. the values for `--username`, `--password`, `--batchaccountkey`, `--storageaccountkey`, `--servicebustopicconnection` and  `--eventhubconnectionstring` will be encrypted. Use a `--settingsfilekey` value to manage encryption or use `--keyvaultname` to save them to Azure Key Vault
2. If there are duplicate values in the `--settingsfile` and the command line, the command line argument will take precedence.


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

