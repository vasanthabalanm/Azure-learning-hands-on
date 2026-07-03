using System.Net;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace NotificationFunctionApp.Functions
{
    public class TestQueueSender
    {
        private readonly ILogger<TestQueueSender> _logger;

        public TestQueueSender(ILogger<TestQueueSender> logger)
        {
            _logger = logger;
        }

        [Function("TestQueueSender")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "test/queue")] HttpRequestData req)
        {
            _logger.LogInformation("Sending test message to test-queue");

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true";
            var queueClient = new QueueClient(connectionString, "test-queue");
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync("Test message from sender!");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Test message sent to test-queue");
            return response;
        }
    }
}
