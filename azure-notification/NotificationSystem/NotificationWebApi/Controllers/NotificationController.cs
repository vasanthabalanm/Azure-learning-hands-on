using Microsoft.AspNetCore.Mvc;
using NotificationWebApi.Models;
using System.Text;
using System.Text.Json;

namespace NotificationWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationController> _logger;
        private readonly IConfiguration _configuration;

        public NotificationController(
            IHttpClientFactory httpClientFactory,
            ILogger<NotificationController> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Send Email Notification
        /// </summary>
        [HttpPost("send-email")]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            var notification = new NotificationMessage
            {
                UserId = request.UserId,
                Email = request.Email,
                Subject = request.Subject,
                Body = request.Body,
                NotificationType = "Email"
            };

            return await SendToAzureFunction(notification);
        }

        /// <summary>
        /// Send SMS Notification
        /// </summary>
        [HttpPost("send-sms")]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status202Accepted)]
        public async Task<IActionResult> SendSms([FromBody] SmsRequest request)
        {
            var notification = new NotificationMessage
            {
                UserId = request.UserId,
                PhoneNumber = request.PhoneNumber,
                Subject = request.Subject,
                Body = request.Body,
                NotificationType = "SMS"
            };

            return await SendToAzureFunction(notification);
        }

        /// <summary>
        /// Send Push Notification
        /// </summary>
        [HttpPost("send-push")]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status202Accepted)]
        public async Task<IActionResult> SendPush([FromBody] PushRequest request)
        {
            var notification = new NotificationMessage
            {
                UserId = request.UserId,
                Subject = request.Subject,
                Body = request.Body,
                NotificationType = "Push"
            };

            return await SendToAzureFunction(notification);
        }

        /// <summary>
        /// Send Any Type of Notification (Generic)
        /// </summary>
        [HttpPost("send")]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status202Accepted)]
        public async Task<IActionResult> SendNotification([FromBody] NotificationMessage notification)
        {
            return await SendToAzureFunction(notification);
        }

        private async Task<IActionResult> SendToAzureFunction(NotificationMessage notification)
        {
            try
            {
                var azureFunctionUrl = _configuration["AzureFunction:NotificationUrl"]
                    ?? "http://localhost:7071/api/notifications/send";

                _logger.LogInformation($"Sending {notification.NotificationType} notification to Azure Function");

                var client = _httpClientFactory.CreateClient();
                var json = JsonSerializer.Serialize(notification);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(azureFunctionUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<NotificationResponse>(responseContent);
                    _logger.LogInformation($"Notification queued successfully. MessageId: {result?.MessageId}");

                    // Return 202 Accepted with the result
                    return StatusCode(StatusCodes.Status202Accepted, result);
                }

                _logger.LogError($"Failed to send notification. Status: {response.StatusCode}");
                return StatusCode((int)response.StatusCode, new { error = responseContent });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Notification Status (placeholder for future implementation)
        /// </summary>
        [HttpGet("status/{messageId}")]
        public IActionResult GetNotificationStatus(string messageId)
        {
            // TODO: Implement status tracking
            return Ok(new { messageId, status = "queued", message = "Status tracking coming soon" });
        }
    }

    // Request Models for Swagger UI
    public class EmailRequest
    {
        public string? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class SmsRequest
    {
        public string? UserId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class PushRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
