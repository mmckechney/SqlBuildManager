<#
.SYNOPSIS
    Cleans up Kubernetes resources left behind by integration tests.
.DESCRIPTION
    Deletes the Kubernetes job, secrets, config maps, Azure identity, identity binding,
    and secret provider class resources from the cluster that were created during test runs.
#>
kubectl delete job sqlbuildmanager
kubectl delete secret connection-secrets
kubectl delete configmap runtime-properties
$identity =(kubectl get AzureIdentity -o name)
kubectl delete $identity
kubectl delete AzureIdentityBinding azure-pod-identity-binding
kubectl delete SecretProviderClass azure-kvname

