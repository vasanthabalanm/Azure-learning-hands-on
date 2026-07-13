using AppointmentQueueAPI.Data;
using AppointmentQueueAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Enhanced Swagger configuration with XML documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SSE Appointment Queue API",
        Version = "v1",
        Description = "Real-time appointment queue system using Server-Sent Events (SSE)",
        Contact = new OpenApiContact
        {
            Name = "SSE Learning Demo",
            Url = new Uri("http://localhost:4200")
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add JWT/Bearer auth scheme (for future use)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header"
    });
});

// PostgreSQL via EF Core
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AppointmentService is Scoped (uses DbContext which is scoped per request)
builder.Services.AddScoped<AppointmentService>();

// SseService stays Singleton (holds all active SSE connections)
builder.Services.AddSingleton<SseService>();

// CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
    db.Database.Migrate();
    Console.WriteLine("✅ Database migrations applied successfully!");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Appointment Queue API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "SSE Appointment Queue API";
});

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Urls.Add("http://localhost:5000");

Console.WriteLine("🚀 Starting server on http://localhost:5000");
Console.WriteLine("📚 Swagger UI: http://localhost:5000/swagger");
Console.WriteLine("🔌 SSE Stream: http://localhost:5000/api/sse/stream");

app.Run();
