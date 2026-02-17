[CmdletBinding()]
param (
    [Parameter()]
    [string] $prefix 
)

.\run_tests_in_aci.ps1 -prefix $prefix -customName batchqueue -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Queue" -timeoutMinutes 300
.\run_tests_in_aci.ps1 -prefix $prefix -customName batchoverride -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Override" -timeoutMinutes 300
.\run_tests_in_aci.ps1 -prefix $prefix -customName batchquery -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Query" -timeoutMinutes 300
.\run_tests_in_aci.ps1 -prefix $prefix -customName containerapp -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.ContainerAppTests" -timeoutMinutes 300
.\run_tests_in_aci.ps1 -prefix $prefix -customName aci -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.AciTests" -timeoutMinutes 300
.\run_tests_in_aci.ps1 -prefix $prefix -customName aks -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.KubernetesTests" -timeoutMinutes 300

if( (Test-Path ./testresults) -eq $false) { mkdir testresults }
az storage blob download-batch --account-name "$($prefix)storage" --source testresults --destination ./testresults  --auth-mode login