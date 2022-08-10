# Key Concepts

Below are some high level concepts used by SQL Build Manager. You will see these used through out the documents and how-to's so it is important to understand what they mean:

## "Build"
The action of updating to your database or fleet of databases with SQL Build Manager. Your build is wrapped in an all-or-nothing transaction meaning if a script fails, your database will be rolledback to the state it was prior to your build. The app also maintains a build history in the database by adding a logging table to the database.


## "Package"

The bundling of your scripts for updating your databases. In addition to managing the order of script execution, it also manages meta-data about the scripts such as a desription, the author and unique id. The package also maintains hash values of each script and the package as a whole to ensure the integrity and allow tracking. 

## "Override" file

The list of servers/databases you want to update with a build. When a package is created, the scripts are assigned a default database named "client". The override file is a list of the SQL Server Name and database targets that will replace "client" at build time. This file is in a format of:
`{sql server name}:client,{target database}` with one target per line. 

## Remote Build Execution

The ability to distribute the load across multiple compute nodes. There are four options with SQL Build Manager: Azure Batch, Azure Kubernetes Service, Azure Container Apps and Azure Container Instance. Each of these has some specifif configuration required, but have simiar process steps


## "Settings" file

A configuration file that can be saved and re-used across multiple builds. It saves configurations for your remote envionment, identities, container names,  connection strings, etc. Any sensitive information is encrypted with AES265 encryption with the value you provide with the `--settingsfilekey`. Sensitive information can instead be stored in Azure Key Vault with a `--keyvault` parameter if needed