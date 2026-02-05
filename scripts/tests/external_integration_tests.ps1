[CmdletBinding()]
param (
    [Parameter()]
    [TypeName]
    $prefix
)

.\run_tests_in_aci.ps1 -prefix $prefix -customName batchqueue -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Queue"
.\run_tests_in_aci.ps1 -prefix $prefix -customName batchoverride -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Override"
.\run_tests_in_aci.ps1 -prefix $prefix -customName batchoquery-testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Query"
.\run_tests_in_aci.ps1 -prefix $prefix -customName containerapp -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.ContainerAppTests"
.\run_tests_in_aci.ps1 -prefix $prefix -customName aci -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.AciTests"
.\run_tests_in_aci.ps1 -prefix $prefix -customName aks -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.KubernetesTests"