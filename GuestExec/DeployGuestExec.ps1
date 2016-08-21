#-- Reset local cluster
cd "C:\Program Files\Microsoft SDKs\Service Fabric\ClusterSetup\"
#.\DevClusterSetup.ps1 -AsSecureCluster  #create a secure cluster
.\DevClusterSetup.ps1

Connect-ServiceFabricCluster localhost:19000

Write-Host 'Copying application package...'
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath 'C:\github\servicefabricdemos\GuestExec' -ImageStoreConnectionString 'file:C:\SfDevCluster\Data\ImageStoreShare' -ApplicationPackagePathInImageStore 'Store\NodeApp'

Write-Host 'Registering application type...'
Register-ServiceFabricApplicationType -ApplicationPathInImageStore 'Store\NodeApp'

Write-Host 'Creating new Application Instance...'
New-ServiceFabricApplication -ApplicationName 'fabric:/NodeApp' -ApplicationTypeName 'NodeAppType' -ApplicationTypeVersion 1.0

Write-Host 'Creating new Service Instance...'
New-ServiceFabricService -ApplicationName 'fabric:/NodeApp' -ServiceName 'fabric:/NodeApp/NodeAppService' -ServiceTypeName 'NodeApp' -Stateless -PartitionSchemeSingleton -InstanceCount 1


Write-Host 'Remove application...'
Remove-ServiceFabricApplication 'fabric:/NodeApp'

Write-Host 'Unregister application type...'
Unregister-ServiceFabricApplicationType -ApplicationTypeName 'NodeAppType' -ApplicationTypeVersion 1.0