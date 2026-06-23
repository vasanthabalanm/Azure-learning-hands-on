# Azure MSAL Multi-Tenant Demo

A corporate-standard starter application demonstrating Azure AD (Microsoft Entra ID) authentication with multi-tenant support and role-based access control.

## Tech Stack

| Layer    | Technology                          |
|----------|-------------------------------------|
| Frontend | Angular 18 + MSAL Angular 3.x       |
| Backend  | ASP.NET Core 8 Web API              |
| Database | PostgreSQL + Entity Framework Core  |
| Identity | Microsoft Entra ID (Azure AD)       |

## Project Structure

```
MSAL-setup/
├── backend/
│   ├── MsalDemo.sln
│   └── MsalDemo.Api/
│       ├── Authorization/        # Role constants and policies
│       ├── Controllers/          # API endpoints by role
│       ├── Data/                 # EF Core DbContext and seeder
│       ├── Entities/             # Database models
│       └── appsettings.json      # Configuration
└── frontend/
    ├── src/
    │   ├── app/
    │   │   ├── core/             # Services and guards
    │   │   └── pages/            # Route components
    │   └── environments/         # MSAL configuration
    └── package.json
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- Azure subscription with permission to create app registrations

---

## Azure AD Setup

### Step 1: Register the API Application

1. Go to [Azure Portal](https://portal.azure.com) → **Microsoft Entra ID** → **App registrations** → **New registration**
2. Configure:
   - **Name**: `MsalDemo-API`
   - **Supported account types**: *Accounts in any organizational directory (Multi-tenant)*
   - **Redirect URI**: Leave blank
3. After creation, note the **Application (client) ID**
4. Go to **Expose an API**:
   - Click **Set** next to Application ID URI → Accept default `api://{client-id}`
   - Click **Add a scope**:
     - Scope name: `access_as_user`
     - Who can consent: Admins and users
     - Display names/descriptions: Fill as needed
5. Go to **App roles** → **Create app role** (repeat for each role):

   | Display Name | Value    | Allowed member types |
   |--------------|----------|----------------------|
   | Admin        | Admin    | Users/Groups         |
   | Manager      | Manager  | Users/Groups         |
   | User         | User     | Users/Groups         |

### Step 2: Register the SPA Application

1. Create another app registration:
   - **Name**: `MsalDemo-SPA`
   - **Supported account types**: *Accounts in any organizational directory (Multi-tenant)*
   - **Redirect URI**: Select **Single-page application (SPA)** → `http://localhost:4200`
2. Note the **Application (client) ID**
3. Go to **API permissions**:
   - Click **Add a permission** → **My APIs** → Select `MsalDemo-API`
   - Check `access_as_user` → **Add permissions**
4. Click **Grant admin consent** (if you have permissions)

### Step 3: Assign Roles to Users

1. Go to **Enterprise applications** → Find `MsalDemo-API`
2. Go to **Users and groups** → **Add user/group**
3. Select users and assign roles (Admin, Manager, or User)

---

## Backend Setup

### 1. Update Configuration

Edit `backend/MsalDemo.Api/appsettings.Development.json`:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "common",
    "ClientId": "YOUR_API_CLIENT_ID",
    "Audience": "api://YOUR_API_CLIENT_ID"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=msal_demo_dev;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 2. Create PostgreSQL Database

```bash
psql -U postgres
CREATE DATABASE msal_demo_dev;
\q
```

### 3. Run Migrations and Start API

```bash
cd backend/MsalDemo.Api

# Install EF Core tools (first time only)
dotnet tool install --global dotnet-ef

# Create and apply migration
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run the API
dotnet run
```

The API will start at `https://localhost:7001` (or check console output).

---

## Frontend Setup

### 1. Update Configuration

Edit `frontend/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  msalConfig: {
    auth: {
      clientId: 'YOUR_SPA_CLIENT_ID',
      authority: 'https://login.microsoftonline.com/common',
      redirectUri: 'http://localhost:4200'
    }
  },
  apiConfig: {
    scopes: ['api://YOUR_API_CLIENT_ID/access_as_user'],
    uri: 'https://localhost:7001/api'
  }
};
```

### 2. Install Dependencies and Run

```bash
cd frontend
npm install
npm start
```

Open `http://localhost:4200` in your browser.

---

## Testing the Application

### Sign In Flow

1. Click **Sign In with Microsoft**
2. Authenticate with your organizational account
3. Consent to permissions (first time)
4. You'll be redirected back with your profile visible

### Role-Based Access

| Page         | URL            | Required Role            |
|--------------|----------------|--------------------------|
| Home         | `/`            | Public                   |
| Profile      | `/profile`     | Any authenticated user   |
| User Area    | `/user`        | User, Manager, or Admin  |
| Manager Area | `/manager`     | Manager or Admin         |
| Admin Area   | `/admin`       | Admin only               |

### API Endpoints

| Endpoint                    | Method | Policy         |
|-----------------------------|--------|----------------|
| `/api/health`               | GET    | Public         |
| `/api/me`                   | GET    | Authenticated  |
| `/api/user/tasks`           | GET    | UserOrAbove    |
| `/api/user/notifications`   | GET    | UserOrAbove    |
| `/api/manager/dashboard`    | GET    | ManagerOrAbove |
| `/api/manager/reports`      | GET    | ManagerOrAbove |
| `/api/admin/users`          | GET    | AdminOnly      |
| `/api/admin/audit-logs`     | GET    | AdminOnly      |

---

## Understanding the Code

### Backend: JWT Validation

The API validates tokens issued by Azure AD and extracts role claims:

```csharp
// Program.cs - Authentication setup
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.RequireRole(AppRoles.Admin));
});
```

### Frontend: MSAL Integration

Angular uses MSAL for authentication and automatic token attachment:

```typescript
// main.ts - MSAL configuration
{
  provide: MSAL_INTERCEPTOR_CONFIG,
  useFactory: () => ({
    interactionType: InteractionType.Redirect,
    protectedResourceMap: new Map([
      ['https://localhost:7001/api', ['api://CLIENT_ID/access_as_user']]
    ])
  })
}
```

### Role-Based Route Protection

```typescript
// app.routes.ts
{
  path: 'admin',
  component: AdminComponent,
  canActivate: [MsalGuard, RoleGuard],
  data: { roles: ['Admin'] }
}
```

---

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| CORS errors | Ensure API CORS policy includes `http://localhost:4200` |
| Token not attached | Verify API URL matches `protectedResourceMap` |
| No roles in token | Check app role assignments in Enterprise Applications |
| 401 Unauthorized | Verify `Audience` matches API's `api://` URI |
| 403 Forbidden with roles | Azure AD uses `roles` claim, not standard role claim - see Custom Authorization Handler below |
| MSAL initialization error | Wait for `InteractionStatus.None` before checking roles |
| "No role" flicker on login | Fetch roles before navigating to dashboard |
| **Owner can't login** | Your personal account is in a different tenant - create a user in the same tenant as app registration (see below) |

### Owner/Admin Cannot Login (Tenant Mismatch)

If you created Azure with a personal email (Gmail, Outlook, etc.) but your app is registered in an organization tenant:

1. Go to **Microsoft Entra ID** → **Users** → **New user**
2. Create user with domain `@yourcompany.onmicrosoft.com` (same as app registration)
3. Assign roles in **Enterprise applications** → Your API → **Users and groups**
4. Login with the new user account

See [msal-setup-creation-guide.md](./msal-setup-creation-guide.md#issue-5-tenant-id-mismatch---owner-cannot-login) for detailed explanation.

### Debug Token Claims

Add this to a controller to inspect claims:

```csharp
[HttpGet("debug-claims")]
[Authorize]
public IActionResult DebugClaims()
{
    return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
}
```

### Custom Authorization Handler for Azure AD Roles

Azure AD tokens use `roles` claim instead of the standard ClaimTypes.Role. If `RequireRole()` returns 403 even with correct roles, implement a custom handler:

```csharp
// Authorization/RolesAuthorizationHandler.cs
public class RolesAuthorizationHandler : AuthorizationHandler<RolesRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RolesRequirement requirement)
    {
        var roleClaims = context.User.FindAll("roles").Select(c => c.Value).ToList();
        
        if (requirement.AllowedRoles.Any(role => roleClaims.Contains(role)))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
```

---

## Related Documentation

- [MSAL Setup Creation Guide](./msal-setup-creation-guide.md) - Detailed setup steps with issues and fixes
- [MSAL Beginner Guide](./msal-beginner-guide.md) - Step-by-step manual for newcomers

---

## Next Steps

- [ ] Add refresh token handling
- [ ] Implement user profile photo from Microsoft Graph
- [ ] Add Azure deployment (App Service + Azure Database for PostgreSQL)
- [ ] Configure CI/CD pipeline
- [ ] Add integration tests

## License

This is a learning sample. Use freely for educational purposes.
