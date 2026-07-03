using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NotificationFunctionApp.Functions
{
    public class TestQueueTrigger
    {
        private readonly ILogger<TestQueueTrigger> _logger;

        public TestQueueTrigger(ILogger<TestQueueTrigger> logger)
        {
            _logger = logger;
        }

        [Function("TestQueueTrigger")]
        public void Run([QueueTrigger("test-queue", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            _logger.LogInformation("@@@ TEST QUEUE TRIGGER WORKED! @@@");
            _logger.LogInformation($"Message: {myQueueItem}");
        }
    }
}
