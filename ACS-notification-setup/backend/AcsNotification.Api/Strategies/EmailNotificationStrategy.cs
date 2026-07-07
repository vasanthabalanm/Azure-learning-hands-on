using AcsNotification.Api.Enums;
using Azure;
using Azure.Communication.Email;

namespace AcsNotification.Api.Strategies;

// Real Azure Communication Services Email implementation
public class EmailNotificationStrategy : INotificationStrategy
{
    private readonly EmailClient? _emailClient;
    private readonly string? _senderEmail;
    private readonly ILogger<EmailNotificationStrategy> _logger;
    private readonly bool _isConfigured;

    public EmailNotificationStrategy(IConfiguration configuration, ILogger<EmailNotificationStrategy> logger)
    {
        _logger = logger;

        // Get ACS connection string and sender email from configuration
        var connectionString = configuration["ACS_CONNECTION_STRING"];
        _senderEmail = configuration["ACS_SENDER_EMAIL"];

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(_senderEmail))
        {
            _isConfigured = false;
            _logger.LogWarning(
                "⚠️ EmailNotificationStrategy initialized WITHOUT Azure Communication Services. " +
                "Missing configuration: {MissingConfig}. Email notifications will not be sent.",
                string.IsNullOrEmpty(connectionString) && string.IsNullOrEmpty(_senderEmail) 
                    ? "ACS_CONNECTION_STRING and ACS_SENDER_EMAIL" 
                    : string.IsNullOrEmpty(connectionString) 
                        ? "ACS_CONNECTION_STRING" 
                        : "ACS_SENDER_EMAIL");
        }
        else
        {
            _emailClient = new EmailClient(connectionString);
            _isConfigured = true;
            _logger.LogInformation("✅ EmailNotificationStrategy initialized with Azure Communication Services");
        }
    }

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<bool> SendAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            _logger.LogWarning(
                "⚠️ Cannot send email to {Recipient} - Azure Communication Services not configured. " +
                "Please set ACS_CONNECTION_STRING and ACS_SENDER_EMAIL in your configuration.",
                recipient);
            return false;
        }

        try
        {
            _logger.LogInformation("📧 Sending REAL email via Azure Communication Services to {Recipient}", recipient);

            // Create email content
            var emailContent = new EmailContent("Healthcare Notification")
            {
                PlainText = message,
                Html = $"<html><body><p>{message}</p></body></html>"
            };

            // Create email message
            var emailMessage = new EmailMessage(
                senderAddress: _senderEmail!,
                recipientAddress: recipient,
                content: emailContent);

            // Send email
            EmailSendOperation emailSendOperation = await _emailClient!.SendAsync(
                WaitUntil.Completed,
                emailMessage,
                cancellationToken);

            // Check status
            if (emailSendOperation.HasCompleted)
            {
                var status = emailSendOperation.Value.Status;

                if (status == EmailSendStatus.Succeeded)
                {
                    _logger.LogInformation("✅ EMAIL sent successfully to {Recipient} via Azure ACS", recipient);
                    return true;
                }
                else
                {
                    _logger.LogWarning("⚠️ Email status: {Status} for {Recipient}", status, recipient);
                    return false;
                }
            }

            _logger.LogWarning("⚠️ Email send operation did not complete for {Recipient}", recipient);
            return false;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "❌ Azure Communication Services error sending email to {Recipient}: {Message}",
                recipient, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {Recipient}", recipient);
            return false;
        }
    }
}
