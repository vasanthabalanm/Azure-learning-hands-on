using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NotificationFunctionApp.Models;

namespace NotificationFunctionApp.Functions
{
    public class SendNotificationHttp
    {
        private readonly ILogger<SendNotificationHttp> _logger;
        private readonly QueueClient _queueClient;

        public SendNotificationHttp(ILogger<SendNotificationHttp> logger)
        {
            _logger = logger;

            // Get connection string from configuration
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? "UseDevelopmentStorage=true";

            // Create QueueClient using connection string (much simpler!)
            _queueClient = new QueueClient(connectionString, "notification-queue");

            // Create queue if it doesn't exist
            _queueClient.CreateIfNotExists();
        }

        [Function("SendNotification")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/send")] HttpRequestData req)
        {
            _logger.LogInformation("Received notification request");

            try
            {
                string requestBody = await req.ReadAsStringAsync();

                if (string.IsNullOrEmpty(requestBody))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new { error = "Request body is empty" });
                    return badResponse;
                }

                var notification = JsonSerializer.Deserialize<NotificationMessage>(requestBody);

                if (notification == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new { error = "Invalid request body" });
                    return badResponse;
                }

                notification.MessageId = Guid.NewGuid().ToString();
                notification.CreatedAt = DateTime.UtcNow;

                // Add to queue for processing
                string messageJson = JsonSerializer.Serialize(notification);
                await _queueClient.SendMessageAsync(messageJson);

                _logger.LogInformation($"Notification queued with MessageId: {notification.MessageId}");

                var response = req.CreateResponse(HttpStatusCode.Accepted);
                await response.WriteAsJsonAsync(new
                {
                    MessageId = notification.MessageId,
                    Status = "queued",
                    Message = "Notification queued for processing"
                });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing notification: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
                return errorResponse;
            }
        }
    }
}
