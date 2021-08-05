# Database targeting options

- ### [Override File](#override-file)
  - [File Format](#file-format)
  - [Runtime](#runtime)
- ### [Service Bus Topic](#service-bus-topic)
  - [Background](#background)
  - [Advantages](#advantages)
  - [Queue Runtime](#queue-runtime)

----

## Override file

In order for SQL Build Manager to do its job, you need to tell it what databases you want to update. This is done by sending it list in the form of an `--override` file.

### File Format

The `--override` file consists of one target per line in the format of:

``` bash
<server name>:client,<target database>
```

for example:

``` bash
sqllab1.database.windows.net:client,SqlBuildTest007
sqllab1.database.windows.net:client,SqlBuildTest008
sqllab1.database.windows.net:client,SqlBuildTest009
sqllab1.database.windows.net:client,SqlBuildTest010
```

Why `client`?  Inside the `.sbm` file, there is a default database target set to `client`. If you don't provide an override, it will look for a database of that name. So, that combination of `client,target` tells the builder to substitute the database name `client` with the database name `target` at runtime.

 This is why the flag name is `--override`!

### Runtime

If the `--override` setting is provided but there isn't a `--servicebustopicconnection` value, the runtime will use this file directly to update the database and/or distribute traffic for `threaded` and `batch`. It is important to also understand the impact of [concurrency options](concurrency_options.md).

***NOTE:** The Kubernetes `sbm k8s` and ACI `sbm aci` commands only uses the Service Bus Topic option at runtime.*

----

## Service Bus Topic

### Background

If the value for `--servicebustopicconnection` is set or there is a value for `ServiceBusTopicConnectionString` in the settings JSON file (for batch) or secrets YAML file (for k8s pods), the runtime will look at the Service Bus Topic for messages that contain the override targets.

In order to get the messages into Service Bus, you must first `enqueue` the targets with the `sbm batch enqueue`  or `sbm k8s enqueue` command, passing in the `--override` target file.

Here it is important to also understand the impact of [concurrency options](concurrency_options.md) when enqueueing. If you specify the `--concurrencytype` value of `Server` or `MaxPerServer` the messages are added to a Session enabled Topic subscription so that targeting SQL servers can be controlled. Using the `Count` setting adds the messages to a subscription that is not session enabled to ensure unrestricted message distribution.

### Advantages

A key advantage for using a Service Bus Topic is your ability to easily monitor the progress of the batch run by watching the message count in the Topic. You can use the [Service Bus Explorer app](https://github.com/paolosalvatori/ServiceBusExplorer) or the Service Bus Explorer found in the Azure portal for your Service Bus. When running in Kubernetes, you can should use the `sbm k8s monitor` command.

In addition, it will allow for more even distribution of targets by allowing nodes/pods to not go idle as they would if using an override file and they exhaust their target list.

### Queue Runtime

When executing a run that leverages Service Bus Topics, there are two key considerations.

1. `--concurrencytype` - this must match the value used with enqueueing the messages. If it does not, the runtime will not be able to locate the messages (see background above regarding the targeted subscriptions)
2. `--jobname` - this must match the value used with enqueueing the messages. To ensure that a run only picks up targets that were intended for it, it checks for this value on the message. If the Label on the message does not match the batch job name, that message will be sent to the DeadLetter queue and not processed.
