using OpenSearch.Client;
using OpenSearchDemo.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// OpenSearch Client Configuration
// ─────────────────────────────────────────────────────────────────────────────
var openSearchUri = builder.Configuration["OpenSearch:Uri"] ?? "http://localhost:9200";
var settings = new ConnectionSettings(new Uri(openSearchUri))
    .DefaultIndex(builder.Configuration["OpenSearch:DefaultIndex"] ?? "products")
    .EnableDebugMode()
    .PrettyJson()
    .RequestTimeout(TimeSpan.FromSeconds(30))
    .DisableDirectStreaming(); // Useful for debugging

// Add authentication if configured
var username = builder.Configuration["OpenSearch:Username"];
var password = builder.Configuration["OpenSearch:Password"];
if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
{
    settings.BasicAuthentication(username, password);
}

builder.Services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(settings));

// ─────────────────────────────────────────────────────────────────────────────
// Services Registration
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IOpenSearchService, OpenSearchService>();
builder.Services.AddSingleton<IProductSearchService, ProductSearchService>();

// ─────────────────────────────────────────────────────────────────────────────
// CORS Configuration
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────────────────
// Middleware Pipeline
// ─────────────────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("OpenSearch Demo API");
        options.WithTheme(ScalarTheme.DeepSpace);
    });
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║          OpenSearch Demo API - Ready to Search!                 ║");
Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
Console.WriteLine("║  API Docs:     http://localhost:5000/scalar/v1                  ║");
Console.WriteLine("║  OpenAPI:      http://localhost:5000/openapi/v1.json            ║");
Console.WriteLine("║  Health:       http://localhost:5000/health                     ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");

app.Run();
