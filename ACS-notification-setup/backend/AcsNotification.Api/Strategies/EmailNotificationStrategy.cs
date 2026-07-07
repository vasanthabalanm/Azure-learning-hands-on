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
        // For backward compatibility, parse message if it contains appointment data
        return await SendAsync(recipient, message, null, cancellationToken);
    }

    public async Task<bool> SendAsync(string recipient, string message, object? context, CancellationToken cancellationToken = default)
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
                Html = GenerateEmailTemplate(recipient, message, context)
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
                var messageId = emailSendOperation.Id;

                _logger.LogInformation(
                    "📨 Azure Email Response - Recipient: {Recipient}, Status: {Status}, OperationId: {OperationId}",
                    recipient, status, messageId);

                if (status == EmailSendStatus.Succeeded)
                {
                    _logger.LogInformation("✅ EMAIL sent successfully to {Recipient} via Azure ACS (OperationId: {OperationId})",
                        recipient, messageId);
                    return true;
                }
                else if (status == EmailSendStatus.Failed)
                {
                    _logger.LogError("❌ Email FAILED for {Recipient} - Status: {Status}, OperationId: {OperationId}",
                        recipient, status, messageId);
                    return false;
                }
                else
                {
                    _logger.LogWarning("⚠️ Email status: {Status} for {Recipient}, OperationId: {OperationId}",
                        status, recipient, messageId);
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

    private string GenerateEmailTemplate(string recipient, string message, object? context)
    {
        // Extract data from context if available
        string recipientName = recipient.Split('@')[0];
        string appointmentDate = "22 May 2026";
        string appointmentTime = "10:30 AM";
        string doctor = "Dr. Arun Kumar, MS (Ophth)";
        string department = "General Ophthalmology";
        string tokenNumber = "T-12 · OPD-OPD-2026-1248";
        string hospitalName = "Vasan Eye Care Hospital";
        string hospitalLocation = "Chennai";

        // If context is provided (Appointment object), use real data
        if (context != null)
        {
            var contextType = context.GetType();

            // Try to extract Patient data
            var patientProp = contextType.GetProperty("Patient");
            if (patientProp != null)
            {
                var patient = patientProp.GetValue(context);
                if (patient != null)
                {
                    var firstNameProp = patient.GetType().GetProperty("FirstName");
                    var lastNameProp = patient.GetType().GetProperty("LastName");
                    if (firstNameProp != null && lastNameProp != null)
                    {
                        var firstName = firstNameProp.GetValue(patient)?.ToString() ?? "";
                        var lastName = lastNameProp.GetValue(patient)?.ToString() ?? "";
                        recipientName = $"{firstName} {lastName}".Trim();
                    }
                }
            }

            // Try to extract Appointment data
            var dateProp = contextType.GetProperty("AppointmentDate");
            if (dateProp != null)
            {
                var dateValue = dateProp.GetValue(context);
                if (dateValue is DateTime dt)
                {
                    appointmentDate = dt.ToString("dd MMM yyyy");
                    appointmentTime = dt.ToString("hh:mm tt");
                }
            }

            var doctorProp = contextType.GetProperty("Doctor");
            if (doctorProp != null)
            {
                doctor = doctorProp.GetValue(context)?.ToString() ?? doctor;
            }

            var deptProp = contextType.GetProperty("Department");
            if (deptProp != null)
            {
                department = deptProp.GetValue(context)?.ToString() ?? department;
            }

            var idProp = contextType.GetProperty("Id");
            if (idProp != null)
            {
                var id = idProp.GetValue(context)?.ToString() ?? "";
                if (!string.IsNullOrEmpty(id))
                {
                    // Generate token number from appointment ID
                    var shortId = id.Substring(0, Math.Min(8, id.Length)).ToUpper();
                    var tokenNum = Math.Abs(id.GetHashCode() % 100);
                    tokenNumber = $"T-{tokenNum} · OPD-{shortId}";
                }
            }
        }

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Healthcare Notification</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f5f5f5;"">
    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f5f5f5; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <!-- Main Container -->
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">

                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #2d7c8e 0%, #1e5a6b 100%); padding: 30px 40px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: bold; letter-spacing: 0.5px;"">Vasan Eye Care</h1>
                            <p style=""margin: 8px 0 0 0; color: #e0f2f7; font-size: 13px; letter-spacing: 1px; text-transform: uppercase;"">Excellence in Eye Care - NABH Accredited</p>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 40px 30px 40px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #333333; font-size: 22px; font-weight: 600;"">Your Appointment is Confirmed</h2>

                            <p style=""margin: 0 0 20px 0; color: #555555; font-size: 15px; line-height: 1.6;"">
                                Dear {recipientName},
                            </p>

                            <p style=""margin: 0 0 30px 0; color: #555555; font-size: 15px; line-height: 1.6;"">
                                We are pleased to confirm your appointment at <strong>{hospitalName}, {hospitalLocation}</strong>.
                            </p>

                            <!-- Appointment Details Box -->
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f8fafb; border-left: 4px solid #2d7c8e; border-radius: 6px; margin-bottom: 30px;"">
                                <tr>
                                    <td style=""padding: 20px 25px;"">
                                        <p style=""margin: 0 0 15px 0; color: #2d7c8e; font-size: 14px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;"">
                                            📋 Appointment Details
                                        </p>

                                        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                            <tr>
                                                <td style=""padding: 8px 0; color: #666666; font-size: 14px; vertical-align: top; width: 40%;"">
                                                    <strong>Date:</strong>
                                                </td>
                                                <td style=""padding: 8px 0; color: #333333; font-size: 14px;"">
                                                    {appointmentDate}
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0; color: #666666; font-size: 14px; vertical-align: top;"">
                                                    <strong>Time:</strong>
                                                </td>
                                                <td style=""padding: 8px 0; color: #333333; font-size: 14px;"">
                                                    {appointmentTime}
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0; color: #666666; font-size: 14px; vertical-align: top;"">
                                                    <strong>Doctor:</strong>
                                                </td>
                                                <td style=""padding: 8px 0; color: #333333; font-size: 14px;"">
                                                    {doctor}
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0; color: #666666; font-size: 14px; vertical-align: top;"">
                                                    <strong>Department:</strong>
                                                </td>
                                                <td style=""padding: 8px 0; color: #333333; font-size: 14px;"">
                                                    {department}
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0; color: #666666; font-size: 14px; vertical-align: top;"">
                                                    <strong>Token Number:</strong>
                                                </td>
                                                <td style=""padding: 8px 0; color: #333333; font-size: 14px;"">
                                                    {tokenNumber}
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Instructions -->
                            <p style=""margin: 0 0 10px 0; color: #555555; font-size: 14px; line-height: 1.6;"">
                                Please arrive <strong>10 minutes early</strong> and carry any previous prescriptions or reports.
                            </p>

                            <p style=""margin: 0 0 25px 0; color: #555555; font-size: 14px; line-height: 1.6;"">
                                For rescheduling or queries, call <strong style=""color: #2d7c8e;"">044-28523456</strong> or reply to this email.
                            </p>

                            <!-- Notification Message -->
                            <div style=""background-color: #fff9e6; border-left: 4px solid #ffa500; padding: 15px 20px; border-radius: 6px; margin-bottom: 25px;"">
                                <p style=""margin: 0; color: #666666; font-size: 14px; line-height: 1.6;"">
                                    <strong style=""color: #cc8800;"">📢 Notification:</strong> {message}
                                </p>
                            </div>

                            <p style=""margin: 0; color: #555555; font-size: 14px; line-height: 1.6;"">
                                Warm regards,<br>
                                <strong>Patient Services Team</strong><br>
                                Vasan Eye Care Hospital
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 25px 40px; text-align: center; border-top: 1px solid #e0e0e0;"">
                            <p style=""margin: 0 0 8px 0; color: #666666; font-size: 13px;"">
                                <strong>Vasan Eye Care Hospital</strong>, Anna Nagar, Chennai – 600040
                            </p>
                            <p style=""margin: 0 0 12px 0; color: #2d7c8e; font-size: 13px;"">
                                044-28523456 · contact@veye.in
                            </p>
                            <p style=""margin: 0; color: #999999; font-size: 11px; line-height: 1.5;"">
                                Unsubscribe from appointment reminders
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
