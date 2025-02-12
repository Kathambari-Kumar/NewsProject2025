using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace UpgradeSubscriptionEmail
{
    public class UpgradeEmail
    {
        [FunctionName("UpgradeEmail")]
        public void Run([QueueTrigger("dragonqueue", Connection = "AzureWebJobsStorage")]UpgradeFM myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
