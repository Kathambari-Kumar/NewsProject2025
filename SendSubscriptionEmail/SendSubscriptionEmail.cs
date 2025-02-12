using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace SendSubscriptionToQueue
{
    public static class SendSubscriptionMessage
    {
        [FunctionName("SendSubscriptionMessage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Extract user information
            string name = req.Query["name"];
            string email= req.Query["email"];

            // Parse the request body into a strongly-typed model
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            email = email ?? data?.email;

            string responseMessage;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                responseMessage = "This HTTP triggered function executed successfully. Pass both a name and an email in the query string or in the request body for a personalized response.";
            }
            else
            {
                responseMessage = $"Hello, {name}. Your email is {email}. This HTTP triggered function executed successfully.";
            }

            // Retrieve the connection string for Azure Storage
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config["AzureWebjobsStorage"];

            // Initialize the QueueClient
            QueueClient queueClient = new QueueClient(connectionString, "subscriptionsqueue",
                new QueueClientOptions
                { MessageEncoding = QueueMessageEncoding.Base64 });

            // Ensure the queue exists
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(requestBody);
            // Return  response message
            return new OkObjectResult(responseMessage);
        }
    }
}
