# Azure Communication Service (ACS) Healthcare Notification System

## 📋 Overview

A **Proof of Concept (POC)** healthcare notification system demonstrating multi-channel notifications (Email, SMS, WhatsApp, Push) using the **Strategy Design Pattern** with .NET 8 and PostgreSQL.

This project is designed for **developers** to learn:
- Clean Architecture (Layered Architecture)
- SOLID Principles
- Strategy Pattern (Composition over Inheritance)
- Repository Pattern
- Entity Framework Core with PostgreSQL
- Dependency Injection
- RESTful API design
- Real Azure Communication Services integration (Email)

### 🎯 Use Cases

1. **Follow-Up Notifications**: Send SMS + Email reminders for patient follow-ups
2. **Appointment Notifications**: Send Email + WhatsApp reminders for appointments

---

## 🚦 Implementation Status

| Channel | Status | Implementation |
|---------|--------|----------------|
| **Email** | ✅ **PRODUCTION** | Real Azure Communication Services integration |
| **SMS** | ⚠️ Mock | Console logging (requires paid Azure subscription) |
| **WhatsApp** | ⚠️ Mock | Console logging (requires WhatsApp Business API approval) |
| **Push** | ⚠️ Mock | Console logging (requires Firebase/APNs setup) |

**Email notifications work with real Azure Communication Services!** SMS, WhatsApp, and Push are mock implementations that can be easily swapped with real integrations when ready.

---

## 🏗️ Architecture

```
┌─────────────┐
│ Controllers │  ← API Layer (HTTP endpoints)
└──────┬──────┘
       │
┌──────▼──────┐
│  Services   │  ← Business Logic (orchestration)
└──────┬──────┘
       │
┌──────▼──────┐
│ Strategies  │  ← Strategy Pattern (Email, SMS, WhatsApp, Push)
└──────┬──────┘
       │
┌──────▼──────┐
│Repositories │  ← Data Access Layer
└──────┬──────┘
       │
┌──────▼──────┐
│  DbContext  │  ← Entity Framework Core
└──────┬──────┘
       │
┌──────▼──────┐
│ PostgreSQL  │  ← Database
└─────────────┘
```

### 📦 Project Structure

```
ACS-notification-setup/
├── backend/
│   ├── AcsNotification.Api/           # Main Web API project
│   │   ├── Controllers/               # API endpoints
│   │   ├── Services/                  # Business logic
│   │   ├── Strategies/                # Notification channels (Strategy Pattern)
│   │   ├── Repositories/              # Data access
│   │   ├── Entities/                  # Database models
│   │   ├── Data/                      # DbContext & seeding
│   │   ├── DTOs/                      # Request/Response objects
│   │   ├── Enums/                     # NotificationChannel enum
│   │   └── Migrations/                # EF Core migrations
│   ├── AcsNotification.sln
│   └── .env                           # Database connection string
├── usecase-for-acs-setup.md
└── README.md                          # This file
```

---

## 🚀 Getting Started

### Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL 14+** - [Download](https://www.postgresql.org/download/)
- **Visual Studio Code** or **Visual Studio 2022** (optional)
- **Git** (optional)

### 🔧 Setup Instructions

#### 1. Clone or Navigate to the Project

```bash
cd d:/personal/Azure-leaaning/Azure-learning-hands-on/ACS-notification-setup
```

#### 2. Configure Environment Variables

Edit `backend/.env` file:

```env
# PostgreSQL Database
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=AzureCommnunicationServiceNotification;Username=postgres;Password=YOUR_PASSWORD

# Azure Communication Services (for Email)
ACS_CONNECTION_STRING=endpoint=https://your-acs-resource.communication.azure.com/;accesskey=YOUR_ACCESS_KEY
ACS_SENDER_EMAIL=DoNotReply@your-domain.azurecomm.net
```

**Required:**
- Replace `YOUR_PASSWORD` with your PostgreSQL password

**Optional (for real Email notifications):**
- Replace `ACS_CONNECTION_STRING` with your Azure Communication Service connection string
- Replace `ACS_SENDER_EMAIL` with your Azure-managed domain email
- See **Azure Setup Guide** section below for detailed instructions

#### 3. Restore Dependencies

```bash
cd backend
dotnet restore
```

#### 4. Build the Project

```bash
dotnet build
```

#### 5. Run the Application

```bash
cd AcsNotification.Api
dotnet run
```

The application will:
- ✅ Apply database migrations automatically
- ✅ Seed sample data (2 patients, 1 follow-up, 1 appointment)
- ✅ Start the API on `https://localhost:5001` or `http://localhost:5000`

#### 6. Access Swagger UI

Open your browser and navigate to:

```
https://localhost:5001
```

Or:

```
http://localhost:5000
```

You'll see the Swagger UI with all available endpoints.

---

## 🧪 Testing the API

### Demo Scenario 1: Follow-Up Notification (Email + SMS)

1. **Get all pending follow-ups**
   ```
   GET /api/followup/pending
   ```
   Copy the `id` from the response.

2. **Get follow-up details**
   ```
   GET /api/followup/{id}
   ```

3. **Trigger notification**
   ```
   POST /api/followup/trigger-notification
   Body:
   {
     "followUpId": "paste-id-here"
   }
   ```

4. **What happens:**
   - ✅ **Email**: Sent via Azure Communication Services (check inbox/spam folder!)
   - ⚠️ **SMS**: Mock - shows in console: `📱 SMS sent to +1234567890: ...`

### Demo Scenario 2: Appointment Notification (Email + WhatsApp)

1. **Get upcoming appointments**
   ```
   GET /api/appointment/upcoming
   ```
   Copy the `id` from the response.

2. **Get appointment details**
   ```
   GET /api/appointment/{id}
   ```

3. **Trigger notification**
   ```
   POST /api/appointment/trigger-notification
   Body:
   {
     "appointmentId": "paste-id-here"
   }
   ```

4. **What happens:**
   - ✅ **Email**: Sent via Azure Communication Services (check inbox/spam folder!)
   - ⚠️ **WhatsApp**: Mock - shows in console: `💬 WHATSAPP sent to +0987654321: ...`

### Verify Database Logs

Connect to PostgreSQL and run:

```sql
SELECT * FROM "NotificationLogs" ORDER BY "SentAt" DESC;
```

You should see all sent notifications with timestamps and success status.

---

## 🎓 Learning Points

### 1. **Strategy Pattern** (Composition over Inheritance)

**Location**: `Strategies/` folder

The Strategy Pattern allows dynamic selection of notification channels at runtime:

- **Interface**: `INotificationStrategy`
- **Implementations**: `EmailNotificationStrategy`, `SmsNotificationStrategy`, `WhatsAppNotificationStrategy`, `PushNotificationStrategy`
- **Orchestrator**: `NotificationService` injects `IEnumerable<INotificationStrategy>` and selects strategies based on enabled channels

**Why Strategy Pattern?**
- ✅ Open/Closed Principle: Add new channels without modifying existing code
- ✅ Single Responsibility: Each strategy handles one channel
- ✅ Testability: Easy to mock strategies
- ✅ No inheritance: Uses composition

### 2. **Repository Pattern**

**Location**: `Repositories/` folder

Separates data access logic from business logic:

- **Generic**: `IRepository<T>` for common CRUD operations
- **Specific**: `IPatientRepository`, `IFollowUpRepository`, `IAppointmentRepository` for custom queries

**Benefits**:
- ✅ Testability: Can mock repositories
- ✅ Maintainability: Database logic isolated
- ✅ Flexibility: Easy to switch databases

### 3. **Dependency Injection with Scrutor (Assembly Scanning)**

**Location**: `Program.cs`

Uses **Scrutor** for automatic registration - when you add a new strategy, repository, or service, it's automatically registered!

**Before Scrutor** (Manual registration):
```csharp
// Had to manually register each one ❌
builder.Services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, SmsNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, WhatsAppNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, PushNotificationStrategy>();
// Adding new strategy = modify Program.cs
```

**With Scrutor** (Automatic scanning):
```csharp
// Auto-registers ALL strategies ✅
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo<INotificationStrategy>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());
// Adding new strategy = zero changes to Program.cs!
```

**Benefits**:
- ✅ **Open/Closed Principle**: Add new implementations without modifying Program.cs
- ✅ **Less boilerplate**: No manual registration for each class
- ✅ **Convention-based**: Automatically finds and registers by pattern
- ✅ **Reduces errors**: Can't forget to register a new class

### 4. **Entity Framework Core Migrations**

**Location**: `Migrations/` folder

Database schema versioned and repeatable:

```bash
# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update
```

Auto-migration on startup configured in `Program.cs`.

---

## 🛠️ Technology Stack

- **Framework**: .NET 8 (Web API)
- **Database**: PostgreSQL 14+
- **ORM**: Entity Framework Core 8.0
- **API Documentation**: Swagger/OpenAPI
- **Logging**: Microsoft.Extensions.Logging
- **Configuration**: DotNetEnv (.env files)
- **Cloud**: Azure Communication Services (Email)

### NuGet Packages

- `Microsoft.EntityFrameworkCore` (8.0.0)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.0)
- `Swashbuckle.AspNetCore` (6.5.0)
- `DotNetEnv` (3.0.0)
- `Azure.Communication.Email` (1.1.0) - For real email sending
- `Scrutor` (7.0.0) - For assembly scanning and automatic DI registration

---

## ☁️ Azure Communication Services Setup (Optional - Email)

The Email channel is integrated with **Azure Communication Services**. Follow these steps to enable real email sending:

### Prerequisites
- Azure account (free trial works!)
- Azure CLI installed (optional, can use Portal)

### Step 1: Create Email Communication Service

1. Go to [Azure Portal](https://portal.azure.com)
2. Search for **"Email Communication Services"**
3. Click **"Create"**
4. Fill in:
   - **Subscription**: Your Azure subscription
   - **Resource Group**: Create new (e.g., `rg-acs-healthcare-notification`)
   - **Name**: `email-acs-healthcare` (or your choice)
   - **Region**: Select your region
5. Click **"Review + Create"** → **"Create"**

### Step 2: Add Azure Managed Domain

1. Open your **Email Communication Service** (`email-acs-healthcare`)
2. Left menu → **"Provision domains"**
3. Click **"Add domain"** → **"Azure subdomain"**
4. Accept defaults → Click **"Add"**
5. Wait for status to show **"Verified"** (green checkmarks for SPF, DKIM, DKIM2)

### Step 3: Create Communication Service

1. Search for **"Communication Services"** (not Email)
2. Click **"Create"**
3. Fill in:
   - **Subscription**: Same as before
   - **Resource Group**: Same as before
   - **Name**: `acs-healthcare-notification`
   - **Data location**: Select region
4. Click **"Review + Create"** → **"Create"**

### Step 4: Connect Domain to Communication Service

1. Open your **Communication Service** (`acs-healthcare-notification`)
2. Left menu → **"Email"** → **"Domains"**
3. Click **"Connect domain"**
4. Select:
   - **Email Service**: `email-acs-healthcare`
   - **Domain**: Your Azure managed domain
5. Click **"Connect"**
6. Verify status shows **"Connected"**

### Step 5: Get Connection String

1. Still in **Communication Service** (`acs-healthcare-notification`)
2. Left menu → **"Keys"**
3. Copy the **Primary connection string**
4. Copy the **Endpoint**
5. Note your sender email format: `DoNotReply@<your-domain-id>.azurecomm.net`

### Step 6: Update .env File

```env
ACS_CONNECTION_STRING=endpoint=https://acs-healthcare-notification.unitedstates.communication.azure.com/;accesskey=YOUR_KEY
ACS_SENDER_EMAIL=DoNotReply@21f5c8d8-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net
```

### Step 7: Test Email Sending

1. Restart your application
2. Use Swagger to trigger a notification
3. Check your email inbox (and spam folder!)

### 💰 Cost for Email
- **Free Tier**: 500 emails/month
- **After Free Tier**: $0.00025 per email (~$0.25 per 1,000 emails)
- **For POC**: Easily stay within free tier!

---

## 💸 Azure Communication Services - Full Cost Breakdown

| Service | Setup Cost | Free Tier | Pay-As-You-Go | Subscription Required |
|---------|-----------|-----------|---------------|----------------------|
| **Email** | Free | 500/month | $0.00025/email | Any (including free trial) |
| **SMS** | $1-2/month (phone number) | None | $0.0075/message | **Paid only** |
| **Voice** | $1-2/month (phone number) | None | $0.013/minute | **Paid only** |
| **WhatsApp** | Free | None | $0.005-0.009/conversation | **Paid + Meta approval** |

**For Free Azure Trial:**
- ✅ Email works perfectly!
- ❌ SMS requires paid subscription + phone number
- ❌ Voice requires paid subscription + phone number  
- ❌ WhatsApp requires paid subscription + WhatsApp Business API approval

---

## 📊 Database Schema

### Entities

**Patient**
- Id (Guid, PK)
- FirstName, LastName, Email, Phone, WhatsAppNumber
- Navigation: FollowUps[], Appointments[]

**FollowUp**
- Id (Guid, PK), PatientId (FK)
- FollowUpDate, Reason, EnabledChannels (JSON), Status
- Navigation: Patient

**Appointment**
- Id (Guid, PK), PatientId (FK)
- AppointmentDate, Doctor, Department, EnabledChannels (JSON), Status
- Navigation: Patient

**NotificationLog** (Audit trail)
- Id (Guid, PK)
- EntityType, EntityId, Channel, Recipient, Message
- SentAt, Success, ErrorMessage

---

## 🔮 Production Enhancements

This POC demonstrates the Strategy Pattern with Email fully integrated. To make it production-ready for all channels:

### 1. **Add Real SMS Integration** (Requires Paid Subscription)
   - Upgrade Azure subscription to pay-as-you-go
   - Acquire phone number from Azure (~$1-2/month)
   - Install `Azure.Communication.Sms` package
   - Update `SmsNotificationStrategy.cs` with Azure SMS Client
   - **Cost**: ~$2-3/month for testing

### 2. **Add Real WhatsApp Integration** (Complex)
   - Apply for WhatsApp Business API approval (takes days/weeks)
   - Install `Azure.Communication.Messages` package
   - Update `WhatsAppNotificationStrategy.cs` with Azure Advanced Messaging
   - **Cost**: ~$0.005-0.009 per conversation

### 3. **Add Push Notifications**
   - Set up Firebase Cloud Messaging (FCM) or Apple Push Notification Service (APNs)
   - Update `PushNotificationStrategy.cs` with real implementation
   - Requires mobile app integration

### 4. **Background Jobs**
   - Add Hangfire or Quartz for scheduled notifications
   - Implement retry logic for failed notifications
   - Add queue processing (Azure Service Bus or RabbitMQ)

### 5. **Testing**
   - Unit tests for services and strategies
   - Integration tests for API endpoints
   - Load testing for performance

### 6. **Security**
   - Add authentication/authorization (JWT, Azure AD)
   - Rate limiting to prevent spam
   - Input validation and sanitization
   - Secrets management (Azure Key Vault)

### 7. **Templates & Personalization**
   - Store notification templates in database
   - Support parameterized messages (e.g., {{patientName}}, {{appointmentTime}})
   - Multi-language support

### 8. **Monitoring & Observability**
   - Add Application Insights for telemetry
   - Health checks for database and external services
   - Structured logging (Serilog)
   - Alerts for failed notifications

### 9. **Deployment**
   - Dockerize the application
   - Deploy to Azure App Service or Azure Container Apps
   - Use Azure Database for PostgreSQL (Flexible Server)
   - Set up CI/CD pipeline (GitHub Actions or Azure DevOps)

---

## 🐛 Troubleshooting

### Database Connection Issues

**Error**: `Could not connect to PostgreSQL`

**Solution**:
1. Ensure PostgreSQL is running: `pg_isready`
2. Check `.env` file has correct credentials
3. Verify database exists: `psql -U postgres -l`

### Migration Issues

**Error**: `No migrations found`

**Solution**:
```bash
cd AcsNotification.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Build Errors

**Solution**:
```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 📚 Additional Resources

### Learn More

- [Strategy Pattern](https://refactoring.guru/design-patterns/strategy)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SOLID Principles](https://www.digitalocean.com/community/conceptual_articles/s-o-l-i-d-the-first-five-principles-of-object-oriented-design)
- [Azure Communication Services](https://azure.microsoft.com/en-us/products/communication-services/)

### What You'll Learn From This Project

1. ✅ **Explore the Strategy Pattern** - See how `EmailNotificationStrategy` swaps from mock to real Azure integration
2. ✅ **Practice Azure Integration** - Set up real Azure Communication Services for Email
3. ✅ **Add unit tests** - Test `NotificationService` with mock strategies
4. ✅ **Add a new notification channel** - Try adding Slack or Telegram using the same pattern
5. ✅ **Upgrade to SMS** - When you have a paid subscription, swap SMS mock for real implementation
6. ✅ **Build a frontend** - Create React/Angular UI to trigger notifications
7. ✅ **Deploy to Azure** - Use Azure App Service + Azure PostgreSQL

### Completed Features
- ✅ Clean Architecture with layered design
- ✅ Strategy Pattern for notification channels
- ✅ Repository Pattern for data access
- ✅ Real Azure Communication Services (Email)
- ✅ Entity Framework Core with auto-migrations
- ✅ Database seeding with sample data
- ✅ Swagger API documentation
- ✅ Comprehensive logging and error handling

---

## 📝 License

This is a learning project for educational purposes.

---

## 👥 Contributors

Designed for junior developers learning .NET, Design Patterns, and Clean Architecture.

---

## 🙏 Acknowledgments

- Inspired by healthcare notification requirements
- Follows Microsoft .NET best practices
- Demonstrates practical application of Design Patterns

---

**Happy Learning! 🚀**
