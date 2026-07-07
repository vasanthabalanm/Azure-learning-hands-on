using AcsNotification.Api.Data;
using AcsNotification.Api.Repositories;
using AcsNotification.Api.Repositories.Interfaces;
using AcsNotification.Api.Services;
using AcsNotification.Api.Services.Interfaces;
using AcsNotification.Api.Strategies;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from backend directory (parent of this project)
var envPath = Path.Combine(Directory.GetParent(builder.Environment.ContentRootPath)!.FullName, ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);

    // Override connection string from .env
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(connectionString))
    {
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
    }

    // Load Azure Communication Services configuration from .env
    var acsConnectionString = Environment.GetEnvironmentVariable("ACS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(acsConnectionString))
    {
        builder.Configuration["ACS_CONNECTION_STRING"] = acsConnectionString;
    }

    var acsSenderEmail = Environment.GetEnvironmentVariable("ACS_SENDER_EMAIL");
    if (!string.IsNullOrEmpty(acsSenderEmail))
    {
        builder.Configuration["ACS_SENDER_EMAIL"] = acsSenderEmail;
    }
}

// Database Configuration
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(dbConnectionString))
{
    throw new InvalidOperationException("Database connection string not configured. Check .env file.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConnectionString));

// Repository Registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IFollowUpRepository, FollowUpRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// Strategy Pattern Registration
// All strategies registered with the same interface allows NotificationService to inject IEnumerable<INotificationStrategy>
builder.Services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, SmsNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, WhatsAppNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, PushNotificationStrategy>();

// Service Layer Registration
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFollowUpService, FollowUpService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ACS Healthcare Notification API",
        Version = "v1",
        Description = "POC for multi-channel healthcare notifications using Strategy pattern"
    });
});

// CORS configuration
var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins") ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.Split(','))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ACS Notification API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Apply pending migrations
        Console.WriteLine("🔄 Applying database migrations...");
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Migrations applied successfully!");

        // Seed initial data
        Console.WriteLine("🌱 Seeding database...");
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.Run();
