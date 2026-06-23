using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using MsalDemo.Api.Data;
using MsalDemo.Api.Authorization;

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
    app.UseSwagger();
    app.UseSwaggerUI();
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
