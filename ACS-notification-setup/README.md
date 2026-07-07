# Azure Communication Service (ACS) Healthcare Notification System

## 📋 Overview

A **Proof of Concept (POC)** healthcare notification system demonstrating multi-channel notifications (Email, SMS, WhatsApp, Push) using the **Strategy Design Pattern** with .NET 8 and PostgreSQL.

This project is designed for **junior developers** to learn:
- Clean Architecture (Layered Architecture)
- SOLID Principles
- Strategy Pattern (Composition over Inheritance)
- Repository Pattern
- Entity Framework Core with PostgreSQL
- Dependency Injection
- RESTful API design

### 🎯 Use Cases

1. **Follow-Up Notifications**: Send SMS + Email reminders for patient follow-ups
2. **Appointment Notifications**: Send Email + WhatsApp reminders for appointments

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

#### 2. Configure PostgreSQL Connection

Edit `backend/.env` file:

```env
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=acs_notification_dev;Username=postgres;Password=YOUR_PASSWORD
```

Replace `YOUR_PASSWORD` with your PostgreSQL password.

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

4. **Check console logs** - You should see:
   ```
   📧 EMAIL sent to john.doe@example.com: ...
   📱 SMS sent to +1234567890: ...
   ```

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

4. **Check console logs** - You should see:
   ```
   📧 EMAIL sent to jane.smith@example.com: ...
   💬 WHATSAPP sent to +0987654321: ...
   ```

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

### 3. **Dependency Injection (DI)**

**Location**: `Program.cs`

All components registered with DI container:

```csharp
// DbContext
builder.Services.AddDbContext<AppDbContext>(...);

// Repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

// Strategies (multiple implementations of same interface)
builder.Services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, SmsNotificationStrategy>();

// Services
builder.Services.AddScoped<INotificationService, NotificationService>();
```

**Benefits**:
- ✅ Loose coupling
- ✅ Easy testing (mock dependencies)
- ✅ Lifetime management (Scoped, Singleton, Transient)

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

### NuGet Packages

- `Microsoft.EntityFrameworkCore` (8.0.0)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.0)
- `Swashbuckle.AspNetCore` (6.5.0)
- `DotNetEnv` (3.0.0)

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

## 🔮 Future Enhancements

This is a POC with mock implementations. To make it production-ready:

1. **Real Integrations**
   - Replace mock strategies with actual Azure Communication Services
   - Integrate SendGrid/AWS SES for Email
   - Integrate Twilio/Vonage for SMS
   - Integrate WhatsApp Business API

2. **Background Jobs**
   - Add Hangfire/Quartz for scheduled notifications
   - Retry failed notifications automatically

3. **Testing**
   - Unit tests for services and strategies
   - Integration tests for API endpoints
   - Load testing for performance

4. **Security**
   - Add authentication/authorization (JWT, Azure AD)
   - Rate limiting to prevent spam
   - Input validation and sanitization

5. **Templates**
   - Store notification templates in database
   - Support parameterized messages (e.g., {{patientName}})

6. **Monitoring**
   - Add Application Insights for telemetry
   - Health checks for database and external services
   - Structured logging (Serilog)

7. **Deployment**
   - Dockerize the application
   - Deploy to Azure App Service
   - Use Azure Database for PostgreSQL

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

### Next Steps

1. Explore the codebase - Read each file to understand implementation
2. Add a new notification channel (e.g., Slack) using the Strategy Pattern
3. Add unit tests for `NotificationService`
4. Integrate real Azure Communication Services
5. Add a frontend (React/Angular) to trigger notifications via UI

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
