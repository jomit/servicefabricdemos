using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Net;
using System.Text;

namespace MyEngine
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MyEngine : StatefulService
    {
        public MyEngine(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    var partitionKey = (this.Partition.PartitionInfo as Int64RangePartitionInformation).LowKey;

                    ServiceEventSource.Current.ServiceMessage(this, "Partition Key => {1}, Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.", partitionKey);

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context, CancellationToken ct)
        {
            String output = null;
            try
            {
                // Grab the vote item string from a "Vote=" query string parameter 
                HttpListenerRequest request = context.Request;
                String voteItem = request.QueryString["Vote"];
                if (voteItem != null)
                {
                    // TODO: Here, write code to perform the following steps: 
                    // Hint: See the RunAsync method to help you with these steps. 
                    // 1. Get a reference to a reliable dictionary using the 
                    // inherited StateManager. The dictionary should String keys 
                    // and int values; Name the dictionary “Votes” 

                    var voteDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("Votes");

                    // 2. Create a new transaction using the inherited StateManager 
                    // 3. Add the voteItem (with a count of 1) if it doesn’t already 
                    // exist or increment its count if it does exist. 
                    // The code below prepares the HTML response. It gets all the current 
                    // vote items (and counts) and separates each with a break (<br>) 
                    var q = from kvp in voteDictionary.CreateEnumerable()
                                //orderby kvp.Key // Intentionally commented out 
                            select $"Item={kvp.Key}, Votes={kvp.Value}";
                    output = String.Join("<br>", q);
                }
            }
            catch (Exception ex) { output = ex.ToString(); }
            // Write response to client: 
            using (var response = context.Response)
            {
                if (output != null)
                {
                    Byte[] outBytes = Encoding.UTF8.GetBytes(output);
                    response.OutputStream.Write(outBytes, 0, outBytes.Length);
                }
            }
        }

    }
}
