# Fix for DomainNotLinked Error

## Root Cause
You're using the connection string from the **base Communication Service** (`acs-healthcare-notification`), but the domain is linked to the **Email Communication Service** (`email-acs-healthcare`).

## Solution: Get Correct Connection String

### Step 1: Navigate to Email Communication Service
1. Go to Azure Portal: https://portal.azure.com
2. Search for "Email Communication Services"
3. Click on **email-acs-healthcare** (NOT acs-healthcare-notification)

### Step 2: Get Connection String
1. In the left menu, click **"Keys"** (under Settings)
2. You'll see:
   - **Endpoint**: Should be something like `https://email-acs-healthcare.unitedstates.communication.azure.com/`
   - **Primary key**
3. Copy the **Connection string** (format: `endpoint=...;accesskey=...`)

### Step 3: Update .env File
Replace the `ACS_CONNECTION_STRING` with the new one:

```env
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=AzureCommnunicationServiceNotification;Username=postgres;Password=admin
ACS_CONNECTION_STRING=endpoint=https://email-acs-healthcare.unitedstates.communication.azure.com/;accesskey=YOUR_NEW_KEY
ACS_SENDER_EMAIL=DoNotReply@21f5c8d8-e8af-47f2-90f3-d7b9c3c020ba.azurecomm.net
```

### Step 4: Restart Your Application
```bash
cd d:\personal\Azure-leaaning\Azure-learning-hands-on\ACS-notification-setup\backend\AcsNotification.Api
dotnet run
```

### Step 5: Test Email
Use Swagger to trigger the follow-up notification and check if email sends successfully!

---

## Why This Happened
- **Base Communication Service** = For SMS, Voice, Chat
- **Email Communication Service** = For Email only
- Domain gets linked to **Email Communication Service**, not base service
- You were using base service connection string → Domain not found error

## After This Fix
Your emails should send successfully through Azure Communication Services! ✅
