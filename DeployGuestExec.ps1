#-- Reset local cluster
cd "C:\Program Files\Microsoft SDKs\Service Fabric\ClusterSetup\"
#.\DevClusterSetup.ps1 -AsSecureCluster  #create a secure cluster
.\DevClusterSetup.ps1

Connect-ServiceFabricCluster localhost:19000

Write-Host 'Copying application package...'
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath 'C:\SFTraining\Demos\GuestExec' -ImageStoreConnectionString 'file:C:\SfDevCluster\Data\ImageStoreShare' -ApplicationPackagePathInImageStore 'Store\NodeApp'

Write-Host 'Registering application type...'
Register-ServiceFabricApplicationType -ApplicationPathInImageStore 'Store\NodeApp'

New-ServiceFabricApplication -ApplicationName 'fabric:/NodeApp' -ApplicationTypeName 'NodeAppType' -ApplicationTypeVersion 1.0

New-ServiceFabricService -ApplicationName 'fabric:/NodeApp' -ServiceName 'fabric:/NodeApp/NodeAppService' -ServiceTypeName 'NodeApp' -Stateless -PartitionSchemeSingleton -InstanceCount 1



#Unpublish-ServiceFabricApplication -ApplicationName "fabric:/NodeApp"

#Remove-ServiceFabricApplicationType -ApplicationTypeName NodeAppType -ApplicationTypeVersion 1.0