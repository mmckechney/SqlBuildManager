kubectl delete job sqlbuildmanager
kubectl delete secret connection-secrets
kubectl delete configmap runtime-properties
$identity =(kubectl get AzureIdentity -o name)
kubectl delete $identity
kubectl delete AzureIdentityBinding azure-pod-identity-binding
kubectl delete SecretProviderClass azure-kvname

