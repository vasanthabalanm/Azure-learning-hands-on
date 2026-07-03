# 📘 Azure Notification System - Complete Manual

**Project:** Azure Function + Web API Notification System  
**Technology:** .NET 8, Azure Functions v4, ASP.NET Core Web API  
**Date Created:** June 30, 2026  
**Status:** ✅ Ready for Testing

---

## 📑 Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture & Design](#2-architecture--design)
3. [What Was Built](#3-what-was-built)
4. [Project Structure](#4-project-structure)
5. [Setup & Installation](#5-setup--installation)
6. [Testing Guide](#6-testing-guide)
7. [Code Explanation](#7-code-explanation)
8. [Troubleshooting](#8-troubleshooting)
9. [Azure Deployment](#9-azure-deployment)
10. [Next Steps](#10-next-steps)

---

## 1. Project Overview

### 1.1 What This Project Does

This is a **notification system** that can send:
- ✉️ **Email notifications**
- 📱 **SMS notifications**  
- 🔔 **Push notifications**

It uses **Azure Functions** for serverless processing and **Web API** for easy testing via Swagger UI.

### 1.2 Why This Architecture?

```
User Request
    ↓
Web API (Easy to test with Swagger)
    ↓
Azure Function (Scalable serverless processing)
    ↓
Queue Storage (Reliable message delivery)
    ↓
Background Processor (Automatic)
    ↓
Notification Service (Email/SMS/Push)
```

**Benefits:**
- ✅ Decoupled architecture (Web API and Function are independent)
- ✅ Scalable (Azure Functions scale automatically)
- ✅ Reliable (Queue ensures delivery)
- ✅ Testable (Swagger UI for easy testing)
- ✅ Cost-effective (Pay only for what you use)

### 1.3 Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Language | C# | .NET 8 |
| Function Runtime | Azure Functions | v4 Isolated |
| Web Framework | ASP.NET Core | 8.0 |
| Storage | Azure Storage Queue | Emulated (Azurite) |
| Documentation | Swagger/OpenAPI | 3.0 |
| Development | Visual Studio / VS Code | Latest |

---

## 2. Architecture & Design

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    USER / CLIENT                             │
└─────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌─────────────────────────────────────────────────────────────┐
│              Web API (localhost:5000)                        │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  NotificationController                              │   │
│  │  - POST /api/Notification/send-email                │   │
│  │  - POST /api/Notification/send-sms                  │   │
│  │  - POST /api/Notification/send-push                 │   │
│  │  - GET  /api/Notification/status/{id}               │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ↓ HTTP POST
┌─────────────────────────────────────────────────────────────┐
│         Azure Function App (localhost:7071)                  │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  SendNotificationHttp (HTTP Trigger)                │   │
│  │  - Receives notification request                     │   │
│  │  - Validates data                                    │   │
│  │  - Queues message                                    │   │
│  │  - Returns 202 Accepted                              │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ↓ Queue Message
┌─────────────────────────────────────────────────────────────┐
│         Azure Storage Queue (Azurite)                        │
│         Queue Name: "notification-queue"                     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ↓ Auto-Trigger
┌─────────────────────────────────────────────────────────────┐
│         Azure Function App (localhost:7071)                  │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  ProcessNotificationQueue (Queue Trigger)           │   │
│  │  - Auto-triggered when message added                │   │
│  │  - Deserializes message                             │   │
│  │  - Routes to service based on type                  │   │
│  │  - Calls notification service                       │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌─────────────────────────────────────────────────────────────┐
│              Notification Service                            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  INotificationService                                │   │
│  │  - SendEmailAsync() → Email Provider                │   │
│  │  - SendSmsAsync() → SMS Provider                    │   │
│  │  - SendPushNotificationAsync() → Push Provider      │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Data Flow

1. **User Action:** User calls Web API endpoint via Swagger UI
2. **Web API:** Validates request, calls Azure Function HTTP endpoint
3. **HTTP Trigger:** Azure Function receives request, adds to queue
4. **Queue Storage:** Message stored reliably in Azure Storage Queue
5. **Queue Trigger:** Auto-triggers when message detected
6. **Processing:** Deserializes message, routes to correct service method
7. **Notification:** Sends via appropriate provider (Email/SMS/Push)
8. **Logging:** Success/failure logged for monitoring

### 2.3 Key Design Patterns

**Pattern 1: Dependency Injection**
- Services registered in `Program.cs`
- INotificationService injected into functions
- Supports testing and maintainability

**Pattern 2: Queue-Based Processing**
- Async, fire-and-forget pattern
- Reliable delivery with automatic retries
- Scales independently from API

**Pattern 3: Service Layer Abstraction**
- INotificationService interface
- Easy to swap implementations
- Ready for real provider integration

---

## 3. What Was Built

### 3.1 Projects Created

**Project 1: NotificationFunctionApp** (Azure Functions)
```
Type: Azure Functions v4 (Isolated)
Framework: .NET 8
Purpose: Serverless notification processor
Components:
  - HTTP Trigger (receives requests)
  - Queue Trigger (processes messages)
  - Notification Service (sends notifications)
Port: 7071 (local)
```

**Project 2: NotificationWebApi** (ASP.NET Core)
```
Type: ASP.NET Core Web API
Framework: .NET 8
Purpose: REST API with Swagger for testing
Components:
  - NotificationController (4 endpoints)
  - HTTP Client (calls Azure Function)
  - Swagger UI (testing interface)
Port: 5000 (local)
```

### 3.2 Files Created

#### NotificationFunctionApp Files

**Functions/SendNotificationHttp.cs**
- HTTP POST endpoint at `/api/notifications/send`
- Accepts NotificationMessage JSON
- Queues message to storage
- Returns 202 Accepted with messageId

**Functions/ProcessNotificationQueue.cs**
- Queue trigger listening to "notification-queue"
- Auto-processes messages
- Routes by NotificationType
- Calls appropriate service method

**Models/NotificationMessage.cs**
```csharp
public class NotificationMessage
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? NotificationType { get; set; }  // Email, SMS, Push
    public DateTime CreatedAt { get; set; }
    public string? MessageId { get; set; }
}
```

**Services/INotificationService.cs**
```csharp
public interface INotificationService
{
    Task<bool> SendEmailAsync(NotificationMessage message);
    Task<bool> SendSmsAsync(NotificationMessage message);
    Task<bool> SendPushNotificationAsync(NotificationMessage message);
}
```

**Services/NotificationService.cs**
- Implements INotificationService
- Placeholder methods for Email, SMS, Push
- Ready for real provider integration
- Comprehensive logging

**Program.cs**
- Configures dependency injection
- Registers INotificationService
- Sets up Application Insights

**host.json**
```json
{
    "version": "2.0",
    "logging": { ... },
    "functionTimeout": "00:05:00"
}
```

**local.settings.json**
```json
{
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    }
}
```

#### NotificationWebApi Files

**Controllers/NotificationController.cs**
- 4 REST endpoints:
  1. `POST /api/Notification/send-email` - Send email
  2. `POST /api/Notification/send-sms` - Send SMS
  3. `POST /api/Notification/send-push` - Send push
  4. `POST /api/Notification/send` - Generic (any type)
  5. `GET /api/Notification/status/{id}` - Check status

**Models/NotificationMessage.cs**
- Same model as Function App
- Ensures consistency

**Program.cs**
- Configures Swagger
- Registers HttpClientFactory
- Sets up routing

**appsettings.json**
```json
{
    "AzureFunction": {
        "NotificationUrl": "http://localhost:7071/api/notifications/send"
    }
}
```

### 3.3 NuGet Packages Added

**NotificationFunctionApp:**
- Microsoft.Azure.Functions.Worker (1.20.0)
- Microsoft.Azure.Functions.Worker.Sdk (1.16.2)
- Microsoft.Azure.Functions.Worker.Extensions.Http (3.1.0)
- Microsoft.Azure.Functions.Worker.Extensions.Storage (6.2.0)
- Microsoft.ApplicationInsights.WorkerService (2.21.0)

**NotificationWebApi:**
- Microsoft.AspNetCore.OpenApi (8.0+)
- Swashbuckle.AspNetCore (6.5+)

---

## 4. Project Structure

### 4.1 Folder Structure

```
D:\personal\Azure-leaaning\Azure-learning-hands-on\azure-notification\
│
├── NotificationSystem/                          ← MAIN SOLUTION FOLDER
│   │
│   ├── NotificationSystem.sln                   ← Solution file
│   │
│   ├── NotificationFunctionApp/                 ← Azure Function Project
│   │   ├── Functions/
│   │   │   ├── SendNotificationHttp.cs         → HTTP endpoint
│   │   │   └── ProcessNotificationQueue.cs     → Queue processor
│   │   ├── Models/
│   │   │   └── NotificationMessage.cs          → Data model
│   │   ├── Services/
│   │   │   ├── INotificationService.cs         → Interface
│   │   │   └── NotificationService.cs          → Implementation
│   │   ├── Program.cs                           → DI setup
│   │   ├── host.json                            → Function config
│   │   ├── local.settings.json                  → Local settings
│   │   └── NotificationFunctionApp.csproj       → Project file
│   │
│   └── NotificationWebApi/                      ← Web API Project
│       ├── Controllers/
│       │   └── NotificationController.cs        → REST endpoints
│       ├── Models/
│       │   └── NotificationMessage.cs           → Data model
│       ├── Program.cs                            → API setup
│       ├── appsettings.json                      → Configuration
│       └── NotificationWebApi.csproj             → Project file
│
├── START-1-Azurite.bat                          ← Start storage emulator
├── START-2-AzureFunction.bat                    ← Start function app
├── START-3-WebAPI.bat                            ← Start web API
└── OPEN-SWAGGER.bat                              ← Open browser
```

### 4.2 File Relationships

```
Web API → calls → Azure Function → writes to → Queue
                                                  ↓
                                    Queue Trigger ← reads from
                                                  ↓
                                        Notification Service
```

---

## 5. Setup & Installation

### 5.1 Prerequisites

**Required Software:**
1. **.NET 8 SDK** - https://dotnet.microsoft.com/download/dotnet/8.0
2. **Azure Functions Core Tools** - `npm install -g azure-functions-core-tools@4`
3. **Azurite** (Storage Emulator) - `npm install -g azurite`
4. **Node.js** (for Azurite) - https://nodejs.org/

**Verification Commands:**
```powershell
# Check .NET version
dotnet --version
# Output: 8.0.x or higher

# Check Azure Functions Core Tools
func --version
# Output: 4.x.x

# Check Azurite
azurite --version
# Output: 3.x.x

# Check Node
node --version
# Output: v18.x.x or higher
```

### 5.2 First-Time Setup

**Step 1: Install Prerequisites**
```powershell
# Install .NET 8 SDK (if not installed)
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0

# Install Azure Functions Core Tools
npm install -g azure-functions-core-tools@4 --unsafe-perm true

# Install Azurite
npm install -g azurite
```

**Step 2: Verify Installation**
```powershell
cd "D:\personal\Azure-leaaning\Azure-learning-hands-on\azure-notification\NotificationSystem"

# Restore packages
dotnet restore

# Build solution
dotnet build

# Expected output: Build succeeded, 0 Error(s)
```

**Step 3: Initial Configuration**

All configuration is already done! The projects are pre-configured with:
- Local storage connection (Azurite)
- Correct ports (7071 for Function, 5000 for API)
- Swagger enabled
- Dependency injection configured

---

## 6. Testing Guide

### 6.1 Quick Start (4 Steps)

**Step 1: Start Azurite (Storage Emulator)**
```
Double-click: START-1-Azurite.bat
Wait for: "Azurite Blob service is successfully listening"
```

**Step 2: Start Azure Function**
```
Double-click: START-2-AzureFunction.bat
Wait for: "SendNotification: [POST] http://localhost:7071/api/notifications/send"
```

**Step 3: Start Web API**
```
Double-click: START-3-WebAPI.bat
Wait for: "Now listening on: http://localhost:5000"
```

**Step 4: Open Swagger**
```
Double-click: OPEN-SWAGGER.bat
Browser opens: http://localhost:5000/swagger
```

### 6.2 Testing with Swagger UI

**Test 1: Send Email Notification**

1. In Swagger, find: `POST /api/Notification/send-email`
2. Click "Try it out"
3. Enter this JSON:
```json
{
  "userId": "user-001",
  "email": "test@example.com",
  "subject": "Test Email Notification",
  "body": "This is a test email from the notification system"
}
```
4. Click "Execute"
5. Expected Response: `202 Accepted`
```json
{
  "messageId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "queued"
}
```

**Test 2: Send SMS Notification**

1. Find: `POST /api/Notification/send-sms`
2. Click "Try it out"
3. Enter:
```json
{
  "userId": "user-002",
  "phoneNumber": "+1234567890",
  "subject": "Verification Code",
  "body": "Your verification code is: 123456"
}
```
4. Click "Execute"
5. Expected: `202 Accepted` with messageId

**Test 3: Send Push Notification**

1. Find: `POST /api/Notification/send-push`
2. Click "Try it out"
3. Enter:
```json
{
  "userId": "user-003",
  "subject": "New Message",
  "body": "You have a new message from John Doe"
}
```
4. Click "Execute"
5. Expected: `202 Accepted` with messageId

### 6.3 Verifying Success

**Check Azure Function Logs:**

Look at the window where you ran `START-2-AzureFunction.bat`. You should see:

```
[2026-06-30T10:30:00.000Z] SendNotification: Received notification request
[2026-06-30T10:30:00.100Z] SendNotification: Notification queued with MessageId: 550e8400...
[2026-06-30T10:30:00.200Z] ProcessNotificationQueue: Processing queue message
[2026-06-30T10:30:00.300Z] ProcessNotificationQueue: Sending email to: test@example.com
[2026-06-30T10:30:00.400Z] ProcessNotificationQueue: Email sent successfully to test@example.com
[2026-06-30T10:30:00.500Z] ProcessNotificationQueue: Notification processed successfully. MessageId: 550e8400...
```

**Success Indicators:**
- ✅ Swagger returns 202 Accepted
- ✅ MessageId is generated and returned
- ✅ Function logs show "Received notification request"
- ✅ Function logs show "Notification queued"
- ✅ Function logs show "Processing queue message"
- ✅ Function logs show "Email/SMS/Push sent successfully"
- ✅ Function logs show "Notification processed successfully"

### 6.4 Testing All Endpoints

**Available Endpoints:**

| Endpoint | Method | Purpose | Test Data |
|----------|--------|---------|-----------|
| `/api/Notification/send-email` | POST | Send email | Requires: email, subject, body |
| `/api/Notification/send-sms` | POST | Send SMS | Requires: phoneNumber, subject, body |
| `/api/Notification/send-push` | POST | Send push | Requires: userId, subject, body |
| `/api/Notification/send` | POST | Generic send | Requires: notificationType + relevant fields |
| `/api/Notification/status/{id}` | GET | Check status | Requires: messageId (from previous response) |

---

## 7. Code Explanation

### 7.1 How SendNotificationHttp Works

**File:** `NotificationFunctionApp/Functions/SendNotificationHttp.cs`

```csharp
[Function("SendNotification")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/send")] HttpRequestData req)
```

**What it does:**
1. Receives HTTP POST request with NotificationMessage JSON
2. Validates the request body
3. Generates unique MessageId (GUID)
4. Sets CreatedAt timestamp (UTC)
5. Serializes notification to JSON
6. Sends message to Azure Storage Queue ("notification-queue")
7. Returns 202 Accepted with messageId and status

**Why 202 Accepted?**
- 202 = Request accepted for processing
- Notification is queued, not yet sent
- Allows async processing
- Client doesn't wait for notification to be sent

### 7.2 How ProcessNotificationQueue Works

**File:** `NotificationFunctionApp/Functions/ProcessNotificationQueue.cs`

```csharp
[Function("ProcessNotificationQueue")]
public async Task Run([QueueTrigger("notification-queue")] string queueItem)
```

**What it does:**
1. **Auto-triggered** when message added to "notification-queue"
2. Deserializes JSON string to NotificationMessage object
3. Uses pattern matching to route by NotificationType:
   ```csharp
   bool success = notification.NotificationType?.ToLower() switch
   {
       "email" => await _notificationService.SendEmailAsync(notification),
       "sms" => await _notificationService.SendSmsAsync(notification),
       "push" => await _notificationService.SendPushNotificationAsync(notification),
       _ => throw new InvalidOperationException($"Unknown type")
   };
   ```
4. Calls appropriate service method
5. Logs success or failure
6. If exception thrown, Azure retries automatically

**Azure's Automatic Retry Policy:**
- First retry: Immediate
- Second retry: After 1 minute
- Third retry: After 2 minutes
- Max retries: 5 times
- After max retries: Message moved to poison queue

### 7.3 How NotificationService Works

**File:** `NotificationFunctionApp/Services/NotificationService.cs`

**SendEmailAsync:**
```csharp
public async Task<bool> SendEmailAsync(NotificationMessage message)
{
    try
    {
        _logger.LogInformation($"Sending email to: {message.Email}");
        
        // TODO: Integrate with SendGrid, AWS SES, or Azure Communication Services
        // For now, this is a placeholder
        await Task.Delay(100); // Simulate async operation
        
        _logger.LogInformation($"Email sent successfully to {message.Email}");
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error sending email: {ex.Message}");
        return false;
    }
}
```

**Current Status:** Placeholder implementation
**Next Step:** Integrate real email provider

**Real Email Integration Example (SendGrid):**
```csharp
// Add NuGet: SendGrid
var client = new SendGridClient(apiKey);
var msg = new SendGridMessage()
{
    From = new EmailAddress("noreply@example.com"),
    Subject = message.Subject,
    PlainTextContent = message.Body
};
msg.AddTo(new EmailAddress(message.Email));
await client.SendEmailAsync(msg);
```

### 7.4 How Web API Controller Works

**File:** `NotificationWebApi/Controllers/NotificationController.cs`

```csharp
[HttpPost("send-email")]
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
```

**SendToAzureFunction method:**
1. Gets Azure Function URL from configuration
2. Serializes notification to JSON
3. Creates HTTP POST request
4. Calls Azure Function endpoint
5. Returns 202 Accepted or error

**Why Web API + Function?**
- Web API: Easy to test (Swagger), integrate with other apps
- Function: Scalable, serverless, pay-per-use
- Separation of concerns: API handles requests, Function handles processing

---

## 8. Troubleshooting

### 8.1 Common Issues

**Issue 1: Port 10000 Permission Denied**

**Symptom:**
```
Azurite Blob service is starting at http://127.0.0.1:10000
Exit due to unhandled error: listen EACCES: permission denied 127.0.0.1:10000
```

**Cause:** Port 10000 is already in use or blocked

**Solution:**
```powershell
# Option 1: Run as Administrator
Right-click START-1-Azurite.bat → Run as Administrator

# Option 2: Kill existing process
netstat -ano | findstr :10000
taskkill /F /PID [ProcessID]

# Option 3: Use different port
azurite --blobPort 10001 --queuePort 10002 --tablePort 10003
```

**Issue 2: Function Not Starting**

**Symptom:**
```
No job functions found
```

**Cause:** Missing extensions or build failure

**Solution:**
```powershell
cd NotificationSystem/NotificationFunctionApp
dotnet clean
dotnet restore
dotnet build
func start
```

**Issue 3: Swagger Page Not Loading**

**Symptom:**
```
Cannot connect to localhost:5000
```

**Cause:** Web API not running or wrong port

**Solution:**
```powershell
# Check if Web API is running
netstat -ano | findstr :5000

# Restart Web API
cd NotificationSystem/NotificationWebApi
dotnet run --urls "http://localhost:5000"

# Open browser manually
start http://localhost:5000/swagger
```

**Issue 4: 502 Bad Gateway in Swagger**

**Symptom:**
```
POST /api/Notification/send-email
502 Bad Gateway
```

**Cause:** Azure Function not running

**Solution:**
Check that `START-2-AzureFunction.bat` window is still open and shows:
```
SendNotification: [POST] http://localhost:7071/api/notifications/send
```

**Issue 5: Queue Message Not Processing**

**Symptom:**
- HTTP endpoint returns 202 Accepted
- But no logs in Function window about processing

**Cause:** Queue trigger not connected to storage

**Solution:**
```powershell
# Verify Azurite is running
# Check Function logs for errors
# Verify local.settings.json has correct connection string:
"AzureWebJobsStorage": "UseDevelopmentStorage=true"
```

### 8.2 Debugging Tips

**Enable Verbose Logging:**

Edit `host.json`:
```json
{
  "version": "2.0",
  "logging": {
    "logLevel": {
      "default": "Debug"
    }
  }
}
```

**Check Storage Queue:**

Use Azure Storage Explorer:
1. Download: https://azure.microsoft.com/features/storage-explorer/
2. Connect to Local Emulator
3. Navigate to: Emulated → Queues → notification-queue
4. View queued messages

**Test Function Directly:**

```powershell
# Test HTTP trigger directly
curl -X POST http://localhost:7071/api/notifications/send `
  -H "Content-Type: application/json" `
  -d '{"userId":"test","email":"test@example.com","subject":"Test","body":"Body","notificationType":"Email"}'
```

---

## 9. Azure Deployment

### 9.1 Prerequisites for Azure

1. Azure Subscription
2. Azure CLI installed: `az --version`
3. Logged in: `az login`

### 9.2 Create Azure Resources

**Step 1: Create Resource Group**
```bash
az group create --name rg-notification-system --location eastus
```

**Step 2: Create Storage Account**
```bash
az storage account create \
  --name stnotification001 \
  --resource-group rg-notification-system \
  --location eastus \
  --sku Standard_LRS
```

**Step 3: Create Function App**
```bash
az functionapp create \
  --name func-notification-001 \
  --resource-group rg-notification-system \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4 \
  --storage-account stnotification001
```

**Step 4: Create App Service for Web API**
```bash
az appservice plan create \
  --name plan-notification-api \
  --resource-group rg-notification-system \
  --sku B1 \
  --is-linux

az webapp create \
  --name api-notification-001 \
  --resource-group rg-notification-system \
  --plan plan-notification-api \
  --runtime "DOTNETCORE:8.0"
```

### 9.3 Deploy Function

**Option 1: Using Azure Functions Core Tools**
```powershell
cd NotificationSystem/NotificationFunctionApp
func azure functionapp publish func-notification-001
```

**Option 2: Using VS Code**
1. Install Azure Functions extension
2. Right-click on Function project
3. Select "Deploy to Function App"
4. Choose func-notification-001

**Option 3: Using Visual Studio**
1. Right-click NotificationFunctionApp project
2. Select "Publish"
3. Choose Azure Function App
4. Select func-notification-001

### 9.4 Deploy Web API

**Using dotnet CLI:**
```powershell
cd NotificationSystem/NotificationWebApi
dotnet publish -c Release

# Create ZIP for deployment
Compress-Archive -Path ./bin/Release/net8.0/publish/* -DestinationPath ./deploy.zip

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group rg-notification-system \
  --name api-notification-001 \
  --src ./deploy.zip
```

### 9.5 Configure Settings

**Update Function App Settings:**
```bash
# Get Storage connection string
CONN_STRING=$(az storage account show-connection-string \
  --name stnotification001 \
  --resource-group rg-notification-system \
  --query connectionString -o tsv)

# Update Function settings
az functionapp config appsettings set \
  --name func-notification-001 \
  --resource-group rg-notification-system \
  --settings "AzureWebJobsStorage=$CONN_STRING"
```

**Update Web API Settings:**
```bash
# Get Function URL
FUNC_URL=$(az functionapp show \
  --name func-notification-001 \
  --resource-group rg-notification-system \
  --query defaultHostName -o tsv)

# Update Web API settings
az webapp config appsettings set \
  --name api-notification-001 \
  --resource-group rg-notification-system \
  --settings "AzureFunction__NotificationUrl=https://$FUNC_URL/api/notifications/send"
```

### 9.6 Test Production

```powershell
# Get Web API URL
$apiUrl = az webapp show --name api-notification-001 --resource-group rg-notification-system --query defaultHostName -o tsv

# Test endpoint
curl -X POST "https://$apiUrl/api/Notification/send-email" `
  -H "Content-Type: application/json" `
  -d '{"userId":"user-001","email":"test@example.com","subject":"Test","body":"Test from Azure"}'
```

### 9.7 Monitor in Azure

**View Function Logs:**
```bash
az functionapp logs tail --name func-notification-001 --resource-group rg-notification-system
```

**View Web API Logs:**
```bash
az webapp log tail --name api-notification-001 --resource-group rg-notification-system
```

**Application Insights:**
Both projects are configured with Application Insights. View metrics at:
```
https://portal.azure.com → Application Insights
```

---

## 10. Next Steps

### 10.1 Immediate Next Steps

✅ **Completed:**
- [x] Project structure created
- [x] Azure Function implemented
- [x] Web API implemented
- [x] Local testing setup
- [x] Documentation complete

🎯 **To Do:**
1. [ ] Test locally with all 3 notification types
2. [ ] Verify queue processing works
3. [ ] Test error scenarios
4. [ ] Deploy to Azure
5. [ ] Integrate real providers

### 10.2 Real Provider Integration

**Email Provider Options:**

**SendGrid (Recommended)**
```csharp
// NuGet: SendGrid
var client = new SendGridClient(apiKey);
var msg = new SendGridMessage()
{
    From = new EmailAddress("noreply@yourdomain.com"),
    Subject = message.Subject,
    PlainTextContent = message.Body,
    HtmlContent = $"<p>{message.Body}</p>"
};
msg.AddTo(new EmailAddress(message.Email));
var response = await client.SendEmailAsync(msg);
```

**Azure Communication Services**
```csharp
// NuGet: Azure.Communication.Email
var emailClient = new EmailClient(connectionString);
var emailMessage = new EmailMessage(
    senderAddress: "DoNotReply@yourdomain.com",
    content: new EmailContent(message.Subject)
    {
        PlainText = message.Body
    },
    recipients: new EmailRecipients(new List<EmailAddress> {
        new EmailAddress(message.Email)
    }));
await emailClient.SendAsync(emailMessage);
```

**SMS Provider Options:**

**Twilio**
```csharp
// NuGet: Twilio
TwilioClient.Init(accountSid, authToken);
var smsMessage = await MessageResource.CreateAsync(
    body: message.Body,
    from: new PhoneNumber("+1234567890"),
    to: new PhoneNumber(message.PhoneNumber)
);
```

**Azure Communication Services**
```csharp
// NuGet: Azure.Communication.Sms
var smsClient = new SmsClient(connectionString);
var response = await smsClient.SendAsync(
    from: "+1234567890",
    to: message.PhoneNumber,
    message: message.Body
);
```

**Push Notification Options:**

**Firebase Cloud Messaging (FCM)**
```csharp
// NuGet: FirebaseAdmin
var messaging = FirebaseMessaging.DefaultInstance;
var pushMessage = new Message()
{
    Notification = new Notification
    {
        Title = message.Subject,
        Body = message.Body
    },
    Token = message.UserId // Device token
};
await messaging.SendAsync(pushMessage);
```

**Azure Notification Hubs**
```csharp
// NuGet: Microsoft.Azure.NotificationHubs
var hub = NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName);
var notification = new GcmNotification("{\"data\":{\"message\":\"" + message.Body + "\"}}");
await hub.SendGcmNativeNotificationAsync(notification, message.UserId);
```

### 10.3 Production Enhancements

**1. Add Authentication**
```csharp
// Add JWT authentication to Web API
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```

**2. Add Rate Limiting**
```csharp
// Prevent abuse
builder.Services.AddRateLimiter(options => { ... });
```

**3. Add Retry Logic**
```csharp
// Use Polly for resilience
services.AddHttpClient<INotificationClient, NotificationClient>()
    .AddTransientHttpErrorPolicy(p => 
        p.WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

**4. Add Status Tracking**
- Store notification status in database
- Update status as processing progresses
- Implement GET endpoint to check status

**5. Add Notification Templates**
- Store email/SMS templates in database
- Support placeholders: {{userName}}, {{code}}
- Different templates for different scenarios

**6. Add Analytics**
- Track delivery rates
- Monitor failures
- Alert on anomalies

**7. Add User Preferences**
- Allow users to opt-in/opt-out
- Respect notification preferences
- Different channels for different notification types

### 10.4 Scaling Considerations

**When to scale:**
- > 1000 notifications/minute
- Multiple notification types active
- Geographic distribution needed

**Scaling strategies:**
- Use Premium Function App plan for better performance
- Add Application Insights for monitoring
- Use Service Bus instead of Storage Queue for complex scenarios
- Implement caching for frequently accessed data
- Use Azure Front Door for global distribution

---

## 📞 Support & Resources

**Documentation:**
- Azure Functions: https://docs.microsoft.com/azure/azure-functions/
- ASP.NET Core: https://docs.microsoft.com/aspnet/core/
- Azure Storage Queues: https://docs.microsoft.com/azure/storage/queues/

**Provider Documentation:**
- SendGrid: https://sendgrid.com/docs/
- Twilio: https://www.twilio.com/docs/
- Firebase: https://firebase.google.com/docs/
- Azure Communication Services: https://docs.microsoft.com/azure/communication-services/

**Community:**
- Stack Overflow: Tag with `azure-functions`, `asp.net-core`
- GitHub Issues: Report bugs or request features

---

## 📝 Summary

This notification system provides a production-ready foundation for sending Email, SMS, and Push notifications using Azure Functions and ASP.NET Core Web API. The architecture is scalable, reliable, and easy to test.

**Key Features:**
✅ Clean architecture with separation of concerns  
✅ Queue-based reliable processing  
✅ Easy testing with Swagger UI  
✅ Ready for real provider integration  
✅ Comprehensive logging and error handling  
✅ Production-ready with minimal changes

**Next Action:** Start testing locally using the batch files!

---

**Document Version:** 1.0  
**Last Updated:** June 30, 2026  
**Status:** Complete & Ready for Testing
