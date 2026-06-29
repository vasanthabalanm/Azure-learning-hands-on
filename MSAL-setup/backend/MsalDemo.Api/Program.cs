using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using MsalDemo.Api.Data;
using MsalDemo.Api.Authorization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────────────────────
// Authentication: Azure AD / Microsoft Entra ID (Multi-tenant)
// ──────────────────────────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        
        // For multi-tenant apps, we need custom issuer validation
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidIssuers = new[]
        {
            "https://login.microsoftonline.com/{tenantid}/v2.0",
            "https://sts.windows.net/{tenantid}/"
        };
        
        // Custom issuer validator for multi-tenant
        options.TokenValidationParameters.IssuerValidator = (issuer, token, parameters) =>
        {
            // Accept any Azure AD tenant issuer
            if (issuer.StartsWith("https://login.microsoftonline.com/") ||
                issuer.StartsWith("https://sts.windows.net/"))
            {
                return issuer;
            }
            throw new Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException(
                $"Issuer '{issuer}' is not valid.");
        };
        
        // Accept both audience formats (with and without api:// prefix)
        var clientId = builder.Configuration["AzureAd:ClientId"];
        options.TokenValidationParameters.ValidAudiences = new[]
        {
            clientId,
            $"api://{clientId}"
        };
        
        // Map roles from the correct claim
        options.TokenValidationParameters.RoleClaimType = "roles";
    }, options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    });

// ──────────────────────────────────────────────────────────────────────────────
// Authorization: Custom handler + Role-based policies
// ──────────────────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, RolesAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    // Use custom RolesRequirement to explicitly check "roles" claim
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.Requirements.Add(new RolesRequirement(AppRoles.Admin)));

    options.AddPolicy(Policies.ManagerOrAbove, policy =>
        policy.Requirements.Add(new RolesRequirement(AppRoles.Admin, AppRoles.Manager)));

    options.AddPolicy(Policies.UserOrAbove, policy =>
        policy.Requirements.Add(new RolesRequirement(AppRoles.Admin, AppRoles.Manager, AppRoles.User)));
});

// ──────────────────────────────────────────────────────────────────────────────
// Database: PostgreSQL via Entity Framework Core
// ──────────────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ──────────────────────────────────────────────────────────────────────────────
// CORS: Allow Angular dev server
// ──────────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MsalDemo API",
        Version = "v1",
        Description = "Multi-tenant Azure AD protected API with role-based access"
    });

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your Azure AD access token (without 'Bearer' prefix)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ──────────────────────────────────────────────────────────────────────────────
// Middleware Pipeline
// ──────────────────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON endpoint (required for all UI tools)
    app.UseSwagger();
    
    // ┌─────────────────────────────────────────────────────────────────────────┐
    // │ Option 1: Swagger UI - Traditional interactive API documentation        │
    // │ URL: /swagger                                                            │
    // └─────────────────────────────────────────────────────────────────────────┘
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MsalDemo API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "MsalDemo API - Swagger";
    });
    
    // ┌─────────────────────────────────────────────────────────────────────────┐
    // │ Option 2: Scalar - Modern API documentation with dark mode & code gen  │
    // │ URL: /scalar                                                             │
    // │ Features: Dark mode, multiple language examples, search, authentication │
    // └─────────────────────────────────────────────────────────────────────────┘
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("MsalDemo API - Scalar")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .AddPreferredSecuritySchemes(new[] { "Bearer" })
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
    });
    
    // ┌─────────────────────────────────────────────────────────────────────────┐
    // │ Option 3: ReDoc - Beautiful structured API documentation                │
    // │ URL: /redoc                                                              │
    // │ Features: Clean UI, three-panel layout, search, deep linking            │
    // └─────────────────────────────────────────────────────────────────────────┘
    app.UseReDoc(options =>
    {
        options.SpecUrl = "/swagger/v1/swagger.json";
        options.RoutePrefix = "redoc";
        options.DocumentTitle = "MsalDemo API - ReDoc";
        
        // ReDoc configuration options
        options.EnableUntrustedSpec();
        options.ScrollYOffset(10);
        options.HideHostname();
        options.HideDownloadButton();
        options.ExpandResponses("200,201");
        options.RequiredPropsFirst();
        options.NoAutoAuth();
        options.PathInMiddlePanel();
        options.HideLoading();
        options.NativeScrollbars();
        options.SortPropsAlphabetically();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed data on startup (dev only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

app.Run();
