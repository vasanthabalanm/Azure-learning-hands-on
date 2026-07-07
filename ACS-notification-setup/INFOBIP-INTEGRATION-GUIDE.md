# Infobip Integration Guide - SMS + WhatsApp

## 🎯 Goal
Integrate Infobip free trial to enable **real SMS and WhatsApp** notifications in your POC.

---

## Step 1: Create Infobip Account (15 minutes)

### 1.1 Sign Up
You already started this! Go back to the Infobip signup page:
- URL: https://www.infobip.com/signup

### 1.2 Fill Registration Form
- **Company email**: Use your real email
- **First name**: Your first name
- **Last name**: Your last name
- **Country**: India (or your country)
- **Company website**: If you don't have one, use your LinkedIn profile URL or GitHub
- ✅ Accept Terms and Conditions

### 1.3 Email Verification
- Check your email inbox
- Click verification link
- Complete any additional verification steps

### 1.4 Wait for Approval
- **Normal**: Few minutes to 1 hour
- **If delayed**: Check spam folder for approval email
- **Status check**: Login to https://portal.infobip.com

---

## Step 2: Get Infobip Credentials (5 minutes)

Once approved and logged in to Infobip Portal:

### 2.1 Get API Key
1. Navigate to **"Settings"** or **"Developers"** section
2. Find **"API Keys"** or **"API Credentials"**
3. Click **"Create API Key"** or use existing one
4. Copy the **API Key** (starts with something like `App xxx...`)
5. **Save it securely** - you won't see it again!

### 2.2 Get Base URL
- Usually: `https://api.infobip.com` or `https://xxxxx.api.infobip.com`
- Found in same section as API Key
- Copy this URL

### 2.3 Note Your Free Credits
- Check dashboard for free trial credits
- SMS: Usually get X free SMS
- WhatsApp: May need additional setup (see below)

---

## Step 3: WhatsApp Setup (If Using WhatsApp)

### Option A: Test WhatsApp Number (Easiest)
1. In Infobip Portal → **Channels** → **WhatsApp**
2. Look for **"Test Number"** or **"Sandbox"**
3. You'll get a test number to send from
4. Can only send to **your verified phone number**

### Option B: Production WhatsApp (Complex - Skip for POC)
- Requires Facebook Business Manager account
- WhatsApp Business API approval
- Takes days/weeks
- **Recommendation**: Skip this for POC, use Test Number

---

## Step 4: Install Infobip SDK

Open terminal in your project:

```bash
cd d:\personal\Azure-leaaning\Azure-learning-hands-on\ACS-notification-setup\backend\AcsNotification.Api
dotnet add package Infobip.Api.Client
```

---

## Step 5: Update .env Configuration

Add Infobip credentials to `.env` file:

```env
# PostgreSQL Database
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=AzureCommnunicationServiceNotification;Username=postgres;Password=admin

# Azure Communication Services (for Email)
ACS_CONNECTION_STRING=endpoint=https://acs-healthcare-notification.unitedstates.communication.azure.com/;accesskey=YOUR_KEY
ACS_SENDER_EMAIL=DoNotReply@21f5c8d8-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net

# Infobip (for SMS + WhatsApp)
INFOBIP_API_KEY=your-infobip-api-key-here
INFOBIP_BASE_URL=https://api.infobip.com
INFOBIP_SENDER_NUMBER=your-infobip-test-number  # e.g., +1234567890
```

---

## Step 6: Update SmsNotificationStrategy.cs

**Location**: `backend/AcsNotification.Api/Strategies/SmsNotificationStrategy.cs`

**Current Code** (Mock):
```csharp
public class SmsNotificationStrategy : INotificationStrategy
{
    private readonly ILogger<SmsNotificationStrategy> _logger;

    public SmsNotificationStrategy(ILogger<SmsNotificationStrategy> logger)
    {
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.SMS;

    public async Task<bool> SendAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        // Mock implementation
        _logger.LogInformation("📱 SMS sent to {Recipient}: {Message}", recipient, message);
        await Task.CompletedTask;
        return true;
    }
}
```

**New Code** (Real Infobip):
```csharp
using Infobip.Api.Client;
using Infobip.Api.Client.Api;
using Infobip.Api.Client.Model;

public class SmsNotificationStrategy : INotificationStrategy
{
    private readonly ILogger<SmsNotificationStrategy> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _senderNumber;

    public SmsNotificationStrategy(
        IConfiguration configuration,
        ILogger<SmsNotificationStrategy> logger)
    {
        _logger = logger;
        _apiKey = configuration["INFOBIP_API_KEY"] 
            ?? throw new InvalidOperationException("INFOBIP_API_KEY not configured");
        _baseUrl = configuration["INFOBIP_BASE_URL"] 
            ?? "https://api.infobip.com";
        _senderNumber = configuration["INFOBIP_SENDER_NUMBER"] 
            ?? throw new InvalidOperationException("INFOBIP_SENDER_NUMBER not configured");
        
        _logger.LogInformation("✅ SmsNotificationStrategy initialized with Infobip");
    }

    public NotificationChannel Channel => NotificationChannel.SMS;

    public async Task<bool> SendAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = new Configuration
            {
                BasePath = _baseUrl,
                ApiKeyPrefix = "App",
                ApiKey = _apiKey
            };

            var sendSmsApi = new SendSmsApi(configuration);

            var smsRequest = new SmsAdvancedTextualRequest(
                messages: new List<SmsTextualMessage>
                {
                    new SmsTextualMessage(
                        destinations: new List<SmsDestination>
                        {
                            new SmsDestination(to: recipient)
                        },
                        from: _senderNumber,
                        text: message
                    )
                }
            );

            var response = await sendSmsApi.SendSmsMessageAsync(smsRequest);

            if (response.Messages?.Count > 0 && response.Messages[0].Status?.GroupId == 1)
            {
                _logger.LogInformation("✅ SMS sent successfully to {Recipient} via Infobip", recipient);
                return true;
            }
            else
            {
                _logger.LogWarning("⚠️ SMS sending failed: {Status}", response.Messages?[0].Status?.Description);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending SMS via Infobip to {Recipient}", recipient);
            return false;
        }
    }
}
```

---

## Step 7: Update WhatsAppNotificationStrategy.cs (Optional)

**Location**: `backend/AcsNotification.Api/Strategies/WhatsAppNotificationStrategy.cs`

**Note**: WhatsApp with Infobip requires:
1. WhatsApp Business API approval (complex)
2. OR use their test/sandbox environment (limited)

**For POC, you can:**
- **Option A**: Keep WhatsApp as mock for now
- **Option B**: Use Infobip test WhatsApp (can only send to your number)

**If using Option B**, the code is similar to SMS but uses `SendWhatsAppTextMessageApi` instead.

---

## Step 8: Update Program.cs Configuration Loading

Make sure Program.cs loads Infobip config from .env:

```csharp
// Add after Azure Communication Services configuration loading
var infobipApiKey = Environment.GetEnvironmentVariable("INFOBIP_API_KEY");
if (!string.IsNullOrEmpty(infobipApiKey))
{
    builder.Configuration["INFOBIP_API_KEY"] = infobipApiKey;
}

var infobipBaseUrl = Environment.GetEnvironmentVariable("INFOBIP_BASE_URL");
if (!string.IsNullOrEmpty(infobipBaseUrl))
{
    builder.Configuration["INFOBIP_BASE_URL"] = infobipBaseUrl;
}

var infobipSenderNumber = Environment.GetEnvironmentVariable("INFOBIP_SENDER_NUMBER");
if (!string.IsNullOrEmpty(infobipSenderNumber))
{
    builder.Configuration["INFOBIP_SENDER_NUMBER"] = infobipSenderNumber;
}
```

---

## Step 9: Test SMS Sending

### 9.1 Build and Run
```bash
cd d:\personal\Azure-leaaning\Azure-learning-hands-on\ACS-notification-setup\backend\AcsNotification.Api
dotnet build
dotnet run
```

### 9.2 Update Seeded Data Phone Number
Before testing, make sure the seeded patient has a **real phone number** (your phone!):

**Option A**: Update manually in database:
```sql
UPDATE "Patients" 
SET "Phone" = '+91XXXXXXXXXX'  -- Your actual phone number with country code
WHERE "FirstName" = 'John';
```

**Option B**: Update `ManualSeeder.cs` and re-seed:
```csharp
Phone = "+91XXXXXXXXXX",  // Your actual phone number
```

### 9.3 Trigger SMS via Swagger
1. Open https://localhost:5001
2. GET `/api/followup/pending` - Copy a follow-up ID
3. POST `/api/followup/trigger-notification`:
   ```json
   {
     "followUpId": "paste-id-here"
   }
   ```

### 9.4 Check Your Phone!
- You should receive a **real SMS** within seconds! 📱
- Check your phone for the notification

---

## Step 10: Verify Infobip Dashboard

1. Login to Infobip Portal
2. Go to **Analytics** or **Logs**
3. You should see your SMS in the sent messages log
4. Check credits used

---

## 🎉 Success Criteria

After completing all steps:
- ✅ Email sends via Azure Communication Services
- ✅ SMS sends via Infobip (real SMS on your phone!)
- ✅ WhatsApp (optional) sends via Infobip
- ✅ Strategy Pattern works with multiple providers
- ✅ All logs in PostgreSQL NotificationLog table

---

## 🐛 Troubleshooting

### Issue: "INFOBIP_API_KEY not configured"
**Solution**: Check .env file is loaded, API key is correct

### Issue: SMS not received
**Solutions**:
1. Check phone number format (must include country code: +91XXXXXXXXXX)
2. Check Infobip dashboard for delivery status
3. Check free trial credits haven't expired
4. Verify API key permissions

### Issue: "401 Unauthorized"
**Solution**: API key is wrong or expired, regenerate in Infobip Portal

### Issue: WhatsApp not working
**Solution**: 
- WhatsApp requires business approval
- Use test/sandbox environment only for POC
- Or keep WhatsApp as mock for now

---

## 💰 Free Trial Limits

Check your Infobip dashboard for:
- **SMS credits**: Usually get X free SMS (e.g., 100)
- **WhatsApp**: Depends on plan, may be limited
- **Expiry**: Free trial usually valid for 30 days

---

## 📝 Next Steps After Integration

1. Update README.md to document Infobip integration
2. Update PROJECT-SUMMARY.md with multi-provider approach
3. Test all notification channels end-to-end
4. Deploy to Azure (optional)

---

## ✅ Ready to Start?

**Begin with Step 1** - Complete Infobip signup and wait for approval.

Let me know when you get your API credentials, and I'll help with the code integration! 🚀
