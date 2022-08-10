



# Settings file

Because running a remote build can require a lot of parameters, a settings file is configuration file that can be saved and re-used across multiple builds. Is saves configurations in your remote envionment as well as container names,  connection strings, etc. Any sensitive information is encrypted with AES265 encryption with the value you provide with the `--settingsfilekey`. Sensitive information can instead be stored in Azure Key Vault with a `--keyvault` parameter if needed


# Remote Build steps

To understand the process of a remote build, these are the high level conceptual steps and apply to each sub-command `sbm batch|k8s|containerapp|aci`. These steps are encapulated in  the `sbm k8s run` and `sbm containerapp run` commands. 


## 1. `savesettings` 

This creates the settings file that can be re-used for the rest of the steps and potentially for other builds leveraging the same remote environment.