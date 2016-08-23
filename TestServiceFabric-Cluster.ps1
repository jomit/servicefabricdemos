$connection = "localhost:19000"
$timeToRun = 2 #60
$maxStabilizationTimeSecs = 180
$waitTimeBetweenIterationsSec = 60

Connect-ServiceFabricCluster $connection

# Run the chaos test scenario
#-----------------------------------------------

$concurrentFaults = 3

Invoke-ServiceFabricChaosTestScenario `
    -TimeToRunMinute $timeToRun `
    -MaxClusterStabilizationTimeoutSec $maxStabilizationTimeSecs `
    -MaxConcurrentFaults $concurrentFaults `
    -EnableMoveReplicaFaults `
    -WaitTimeBetweenIterationsSec $waitTimeBetweenIterationsSec


# Run the failover test scenario
#-----------------------------------------------

$waitTimeBetweenFaultsSec = 10
$serviceName = "fabric:/Voting/VotingService"

Invoke-ServiceFabricFailoverTestScenario `
    -TimeToRunMinute $timeToRun `
    -MaxServiceStabilizationTimeoutSec $maxStabilizationTimeSecs `
    -WaitTimeBetweenFaultsSec $waitTimeBetweenFaultsSec `
    -ServiceName $serviceName `
    -PartitionKindSingleton   #| out-file C:\SFHackathon\results.txt