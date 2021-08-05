# Concurrency Options for Threaded, Batch and Kubernetes executions

You can control the level of parallel execution with the combination of two arguments: `--concurrency` and `--concurrencytype`. While their meaning for threaded and batch/kubernetes are similar, there are some distinctions and subtleties when used together


- [Option Definitions](#option-definitions)
- [Threaded execution](#threaded-execution)
- [Batch, Kubernetes or ACI Execution (with concurrency scenarios)](#batch-kubernetes-or-aci-execution)

----


## Option definitions

### --concurrencytype

#### Allowed values:

- `Count` - (default) will use the value for `concurrency` as the maximum number of concurrent tasks allowed
- `Server` - When using this value, the `concurrency` value is ignored. Instead, the app will interrogate the database targets and allow one task per SQL Server at a time

    _For example:_ if there are 5 unuque SQL Server targets in the database targets config, there will 1 task per server, up to 5 concurrent tasks total

- `MaxPerServer` - Will interrogate the database targets and allow multiple tasks per server, up to the `concurrency` value.

    _For example:_ If there are 5 unique SQL Server targets in the database targets config file and the `concurrency` flag is set to 3, then it will run 3 tasks per server at a time, up to 15 concurrent tasks (5 servers * 3 tasks per server).

### --concurrency

This argument takes an integer number (default is 8) defining the maximum number of concurrent threads per the set `concurrencytype`

----

## Threaded execution

When running  `sbm threaded run` or `sbm threaded query` the arguments are as described above, Since this is run on a single machine, the maximum number of concurrent tasks are what you would expect with the formulas above.

----

## Batch, Kubernetes or ACI execution

When running `sbm batch run`,  `sbm batch query` or `sbm k8s`, you need to consider that you are also running on more than one machine. The concurrency flags are interpreted **_per batch node/ per pod_** and this needs to be accounted for when calculating your desired concurrency.

Whether you are distributing your batch, kubernetes or ACI load with an `--override` file or `--servicebustopicconnection` (see [details on database targeting options](override_options.md)), the concurrency options are available and perform as described below. However, if using a Service Bus Topic, the overall build may be more efficient as there is a smaller likelihood of nodes/pods going idle.

The scenarios below show examples for `batch` execution, but the calculations are the same when running `k8s`, with the calculation per running Kubernetes pod.

### Consider the following:

- Your database targets configuration has 50 unique SQL Server targets.
- You have 10 Azure Batch nodes

### Scenario 1

You want to run 10 tasks per Batch node regardless of the server targets. For this you would use

``` bash
sbm batch run --concurrencytype Count --concurrency 10  ...
```

When distributing load to the Batch nodes, the algorithm does an equal split of targets. The maximum concurrency here would be 100 (10 Batch nodes * 10 concurrent tasks)

### Scenario 2

You are concerned with over tasking your SQL Servers with too much load and only want to run 1 task per SQL Server at a time. For this you would run

``` bash
sbm batch run --concurrencytype Server ...
```

When distributing load to the Batch nodes, the algorithm will first split by SQL Server name, then attempt to distribute as equally as possible. But depending on the number of databases per server, the load could be somewhat uneven across the 10 nodes. In the case of a perfect splitting, each batch node would be assigned 5 SQL Server targets, with a task per SQL Server. So the maximum concurrency would be 50 -- 5 tasks, (1 per server target) * 10 Batch nodes.

_Remember:_ When using the `Server` value, the `--concurrency` value is ignored and can be left to the default.

### Scenario 3

You want the execution to finish as fast as possible, but still want to have some load control on your SQL Servers. So, you only want 5 tasks per server. For this you would run:

``` bash
sbm batch run --concurrencytype MaxPerServer --concurrency 5 ...
```

As with the `Server` option, the algorithm first splits by SQL Server name and attempts an equal distribution. In this case, with the `concurrency` value of 5, it will run up to 5 tasks per SQL Server giving you up to 250 concurrent tasks (5 server targets * 5 tasks per server * 10 Batch nodes)
