# MSAL Setup Creation Guide

This document captures the complete flow of setting up Azure MSAL authentication with role-based access control, including all issues encountered and their solutions.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Azure AD Configuration](#azure-ad-configuration)
3. [Backend Implementation](#backend-implementation)
4. [Frontend Implementation](#frontend-implementation)
5. [Issues Encountered and Fixes](#issues-encountered-and-fixes)
6. [Testing Checklist](#testing-checklist)

---

## Project Overview

### Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Angular SPA   │────▶│  ASP.NET Core   │────▶│   PostgreSQL    │
│  (Port: 4200)   │     │   (Port: 7001)  │     │  (Port: 5432)   │
└────────┬────────┘     └────────┬────────┘     └─────────────────┘
         │                       │
         │    ┌─────────────────────────────┐
         └───▶│     Microsoft Entra ID      │◀────┘
              │        (Azure AD)           │
              │  Tenant: YOUR_TENANT_ID     │
              └─────────────────────────────┘
```

### App Registrations Required

| App Name | Purpose | Client ID Example |
|----------|---------|-------------------|
| MsalDemo-API | Backend API validation | `171d9cb6-72eb-4a41-b83c-14d4807c66c5` |
| MsalDemo-SPA | Frontend authentication | `de265c95-9f62-4465-8caf-6a4cd974830c` |

---

## Azure AD Configuration

### Step 1: Create API App Registration

1. Navigate to **Azure Portal** → **Microsoft Entra ID** → **App registrations**
2. Click **New registration**
3. Configure:
   - Name: `MsalDemo-API`
   - Supported account types: **Accounts in any organizational directory (Multi-tenant)**
   - Redirect URI: Leave empty
4. After creation, copy the **Application (client) ID**

### Step 2: Expose an API

1. Go to **Expose an API**
2. Click **Set** next to Application ID URI
3. Accept the default: `api://{your-api-client-id}`
4. Click **Add a scope**:
   - Scope name: `access_as_user`
   - Who can consent: **Admins and users**
   - Admin consent display name: `Access MsalDemo API`
   - Admin consent description: `Allows the app to access MsalDemo API on behalf of the signed-in user`

### Step 3: Create App Roles

Go to **App roles** → **Create app role** for each:

| Display Name | Value | Description | Allowed member types |
|--------------|-------|-------------|----------------------|
| Admin | Admin | Full administrative access | Users/Groups |
| Manager | Manager | Team management access | Users/Groups |
| User | User | Basic user access | Users/Groups |

### Step 4: Create SPA App Registration

1. Create new registration:
   - Name: `MsalDemo-SPA`
   - Supported account types: **Accounts in any organizational directory (Multi-tenant)**
   - Redirect URI: **Single-page application (SPA)** → `http://localhost:4200`

2. Go to **API permissions**:
   - Click **Add a permission** → **My APIs** → Select `MsalDemo-API`
   - Check `access_as_user`
   - Click **Grant admin consent**

### Step 5: Assign Roles to Users

1. Go to **Enterprise applications** (not App registrations)
2. Find `MsalDemo-API`
3. Go to **Users and groups** → **Add user/group**
4. Select user and assign role

> **CRITICAL**: Roles must be assigned in **Enterprise applications**, NOT in App registrations!

---

## Backend Implementation

### Key Files Created

```
backend/MsalDemo.Api/
├── Authorization/
│   ├── AppRoles.cs              # Role constants
│   ├── Policies.cs              # Policy names
│   ├── RolesRequirement.cs      # Custom requirement
│   └── RolesAuthorizationHandler.cs  # Custom handler (FIX for 403)
├── Controllers/
│   ├── AuthController.cs        # /api/me endpoint
│   ├── AdminController.cs       # Admin-only endpoints
│   ├── ManagerController.cs     # Manager+ endpoints
│   └── UserController.cs        # User+ endpoints
├── Data/
│   ├── AppDbContext.cs          # EF Core context
│   └── DbSeeder.cs              # Sample data seeder
├── Entities/
│   ├── UserProfile.cs           # User entity
│   └── AuditLog.cs              # Audit entity
└── Program.cs                   # App configuration
```

### Program.cs Key Configuration

```csharp
// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Custom Authorization Handler (REQUIRED for Azure AD roles)
builder.Services.AddSingleton<IAuthorizationHandler, RolesAuthorizationHandler>();

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.Requirements.Add(new RolesRequirement(AppRoles.Admin)));
    
    options.AddPolicy(Policies.ManagerOrAbove, policy =>
        policy.Requirements.Add(new RolesRequirement(AppRoles.Admin, AppRoles.Manager)));
    
    options.AddPolicy(Policies.UserOrAbove, policy =>
        policy.Requirements.Add(new RolesRequirement(AppRoles.Admin, AppRoles.Manager, AppRoles.User)));
});

// CORS
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
```

### appsettings.Development.json

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

---

## Frontend Implementation

### Key Files Created

```
frontend/src/
├── app/
│   ├── core/
│   │   ├── guards/
│   │   │   ├── role.guard.ts        # Role-based route protection
│   │   │   └── no-role.guard.ts     # Redirect users WITH roles from /no-role
│   │   └── services/
│   │       └── role.service.ts      # Role fetching service
│   ├── pages/
│   │   ├── home/
│   │   ├── profile/
│   │   ├── admin/
│   │   ├── manager/
│   │   ├── user/
│   │   └── no-role/
│   ├── app.component.ts             # Login/logout handling
│   └── app.routes.ts                # Route configuration
├── environments/
│   └── environment.ts               # MSAL configuration
└── main.ts                          # MSAL module setup
```

### environment.ts

```typescript
export const environment = {
  production: false,
  msalConfig: {
    auth: {
      clientId: 'YOUR_SPA_CLIENT_ID',
      authority: 'https://login.microsoftonline.com/common',
      redirectUri: 'http://localhost:4200'
    },
    cache: {
      cacheLocation: 'localStorage',
      storeAuthStateInCookie: false
    }
  },
  apiConfig: {
    scopes: ['api://YOUR_API_CLIENT_ID/access_as_user'],
    uri: 'https://localhost:7001/api'
  }
};
```

### main.ts MSAL Setup

```typescript
import { MSAL_INSTANCE, MSAL_GUARD_CONFIG, MSAL_INTERCEPTOR_CONFIG } from '@azure/msal-angular';
import { PublicClientApplication, InteractionType } from '@azure/msal-browser';

export function MSALInstanceFactory(): PublicClientApplication {
  return new PublicClientApplication({
    auth: environment.msalConfig.auth,
    cache: environment.msalConfig.cache
  });
}

export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>();
  protectedResourceMap.set(environment.apiConfig.uri, environment.apiConfig.scopes);
  
  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap
  };
}

// Providers
providers: [
  { provide: MSAL_INSTANCE, useFactory: MSALInstanceFactory },
  { provide: MSAL_GUARD_CONFIG, useFactory: MSALGuardConfigFactory },
  { provide: MSAL_INTERCEPTOR_CONFIG, useFactory: MSALInterceptorConfigFactory },
  { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true },
  MsalService,
  MsalGuard,
  MsalBroadcastService
]
```

---

## Issues Encountered and Fixes

### Issue 1: 403 Forbidden Despite Correct Role Assignment

**Symptom**: User has "Admin" role in Azure AD, token contains role, but API returns 403.

**Root Cause**: ASP.NET Core's `RequireRole()` looks for `ClaimTypes.Role` claim, but Azure AD sends roles in a custom `roles` claim.

**Token Example**:
```json
{
  "roles": ["Admin"],
  "aud": "api://171d9cb6-...",
  "iss": "https://login.microsoftonline.com/..."
}
```

**Fix**: Create custom authorization handler that explicitly checks the `roles` claim.

```csharp
// Authorization/RolesRequirement.cs
public class RolesRequirement : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; }
    
    public RolesRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

// Authorization/RolesAuthorizationHandler.cs
public class RolesAuthorizationHandler : AuthorizationHandler<RolesRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RolesRequirement requirement)
    {
        // Explicitly look for Azure AD "roles" claim
        var roleClaims = context.User.FindAll("roles").Select(c => c.Value).ToList();
        
        if (requirement.AllowedRoles.Any(role => roleClaims.Contains(role)))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

**Registration in Program.cs**:
```csharp
builder.Services.AddSingleton<IAuthorizationHandler, RolesAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    // Use Requirements instead of RequireRole
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.Requirements.Add(new RolesRequirement(AppRoles.Admin)));
});
```

---

### Issue 2: BrowserAuthError - uninitialized_public_client_application

**Symptom**: After login redirect, console shows:
```
BrowserAuthError: uninitialized_public_client_application
```

**Root Cause**: RoleGuard tries to call `acquireTokenSilent()` before MSAL has finished initializing.

**Fix**: Wait for MSAL initialization before checking roles.

```typescript
// role.guard.ts
canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
  // Wait for MSAL to finish any ongoing interaction
  return this.msalBroadcastService.inProgress$.pipe(
    filter((status: InteractionStatus) => status === InteractionStatus.None),
    take(1),
    switchMap(() => {
      const account = this.authService.instance.getActiveAccount();
      if (!account) {
        return of(false);
      }
      return this.roleService.getUserRoles().pipe(
        map(roles => {
          const requiredRoles = route.data['roles'] as string[];
          return requiredRoles.some(role => roles.includes(role));
        })
      );
    })
  );
}
```

---

### Issue 3: "No Role Assigned" Flicker on Login

**Symptom**: After successful login, user briefly sees "No role assigned" message before being redirected to their dashboard.

**Root Cause**: `navigateToDashboard()` was called immediately after login, before roles were fetched from API.

**Fix**: Fetch roles FIRST, then navigate based on roles.

```typescript
// app.component.ts
this.msalBroadcastService.msalSubject$
  .pipe(
    filter((msg: EventMessage) => msg.eventType === EventType.LOGIN_SUCCESS)
  )
  .subscribe(() => {
    this.loadingRoles = true; // Show loading state
    
    // Fetch roles FIRST
    this.roleService.getUserRoles().subscribe({
      next: (roles) => {
        this.loadingRoles = false;
        // THEN navigate based on roles
        this.navigateToDashboard(roles);
      },
      error: () => {
        this.loadingRoles = false;
        this.router.navigate(['/no-role']);
      }
    });
  });
```

**Template**:
```html
<div *ngIf="loadingRoles" class="loading-roles">
  Loading your profile...
</div>
```

---

### Issue 4: Admin Can Access /no-role Page

**Symptom**: Users with roles can manually navigate to `/no-role` page.

**Fix**: Create NoRoleGuard to redirect users WITH roles to their dashboard.

```typescript
// no-role.guard.ts
@Injectable({ providedIn: 'root' })
export class NoRoleGuard implements CanActivate {
  
  canActivate(): Observable<boolean> {
    return this.msalBroadcastService.inProgress$.pipe(
      filter(status => status === InteractionStatus.None),
      take(1),
      switchMap(() => {
        return this.roleService.getUserRoles().pipe(
          map(roles => {
            if (roles.length > 0) {
              // User HAS roles - redirect to their dashboard
              this.navigateToDashboard(roles);
              return false;
            }
            // User has NO roles - allow access to /no-role
            return true;
          })
        );
      })
    );
  }
}
```

**Route Configuration**:
```typescript
{
  path: 'no-role',
  component: NoRoleComponent,
  canActivate: [MsalGuard, NoRoleGuard]  // Add NoRoleGuard
}
```

---

### Issue 5: Tenant ID Mismatch - Owner Cannot Login

**Symptom**: 
- Token validation fails with "audience" or "issuer" mismatch errors
- Owner/Admin who created the Azure subscription cannot login to the app
- Error messages like "AADSTS50020: User account from identity provider does not exist in tenant"

**Root Cause**: 

The Azure subscription owner's account (e.g., `balanm@gmail.com` or `balanm@othercompany.com`) belongs to a **different tenant** than the Azure AD where the app is registered.

```
Example Scenario:
┌─────────────────────────────────────────────────────────────────┐
│  Azure Subscription Owner: balanm@gmail.com                     │
│  Owner's Tenant ID: 11111111-aaaa-bbbb-cccc-111111111111       │
│                                                                 │
│  App Registration Tenant: yourcompany.onmicrosoft.com           │
│  App's Tenant ID: 25680fff-d55e-4bfe-b621-0958708d1098         │
│                                                                 │
│  ❌ Tenant IDs don't match = Login fails!                       │
└─────────────────────────────────────────────────────────────────┘
```

**Why This Happens**:
1. You created an Azure account with a personal email (Gmail, Outlook.com, etc.)
2. Azure created a "personal" Azure AD tenant for you
3. You then created a separate organization tenant (e.g., `yourcompany.onmicrosoft.com`)
4. App registrations are in the organization tenant
5. Your personal account is NOT a member of the organization tenant

**Fix**: Create a user INSIDE the same tenant where your app is registered.

**Step-by-Step Solution**:

1. **Go to Azure Portal** → **Microsoft Entra ID** → **Users**

2. **Click "New user"** → **"Create new user"**

3. **Fill in the details**:
   ```
   User principal name: balan1@yourcompany.onmicrosoft.com
   Display name: Balan Test User
   Password: Auto-generate or set manually
   ```

4. **Note**: The domain MUST be `@yourcompany.onmicrosoft.com` (same as your app registration tenant)

5. **After creation**, go to **Enterprise applications** → Your API app → **Users and groups**

6. **Add the new user** and assign a role (Admin, Manager, or User)

7. **Login with the new user**: `balan1@yourcompany.onmicrosoft.com`

**Configuration for Multi-Tenant vs Single-Tenant**:

**Multi-tenant (allows any organization's users)**:
```json
// appsettings.Development.json
{
  "AzureAd": {
    "TenantId": "common",
    "ClientId": "YOUR_API_CLIENT_ID",
    "Audience": "api://YOUR_API_CLIENT_ID"
  }
}
```

```typescript
// environment.ts
authority: 'https://login.microsoftonline.com/common'
```

**Single-tenant (only your organization's users)**:
```json
// appsettings.Development.json
{
  "AzureAd": {
    "TenantId": "25680fff-d55e-4bfe-b621-0958708d1098",
    "ClientId": "YOUR_API_CLIENT_ID",
    "Audience": "api://YOUR_API_CLIENT_ID"
  }
}
```

```typescript
// environment.ts
authority: 'https://login.microsoftonline.com/25680fff-d55e-4bfe-b621-0958708d1098'
```

**Quick Check - Find Your Tenant ID**:

1. Go to **Azure Portal** → **Microsoft Entra ID** → **Overview**
2. Copy the **Tenant ID** shown
3. Ensure this matches your app registration's directory

**Important Notes**:
- Users created in `yourcompany.onmicrosoft.com` can ONLY login with that domain
- The subscription owner's personal account remains separate
- For development/testing, create test users in the same tenant as your apps

**Multi-tenant (recommended for demos)**:
```json
{
  "AzureAd": {
    "TenantId": "common",
    "ClientId": "YOUR_API_CLIENT_ID",
    "Audience": "api://YOUR_API_CLIENT_ID"
  }
}
```

**Single-tenant (production)**:
```json
{
  "AzureAd": {
    "TenantId": "25680fff-d55e-4bfe-b621-0958708d1098",
    "ClientId": "YOUR_API_CLIENT_ID",
    "Audience": "api://YOUR_API_CLIENT_ID"
  }
}
```

---

## Testing Checklist

### Authentication

- [ ] User can click "Sign In" and redirect to Microsoft login
- [ ] After login, user is redirected back to app
- [ ] User profile displays correctly (name, email)
- [ ] Logout clears session completely

### Authorization

- [ ] User with no roles sees "No role assigned" page
- [ ] User with "User" role can access `/user` but not `/manager` or `/admin`
- [ ] User with "Manager" role can access `/user` and `/manager` but not `/admin`
- [ ] User with "Admin" role can access all pages
- [ ] 403 errors do NOT occur for users with correct roles

### API Integration

- [ ] `/api/me` returns user claims
- [ ] `/api/admin/users` returns 403 for non-admins
- [ ] `/api/admin/users` returns data for admins
- [ ] Database data (users, audit logs) displays correctly

### Edge Cases

- [ ] Direct URL navigation respects role guards
- [ ] Page refresh maintains authentication state
- [ ] Token refresh works for long sessions
- [ ] Admin cannot access `/no-role` page

---

## Quick Reference

### Azure AD Portal Locations

| What | Where |
|------|-------|
| Create App Registration | Entra ID → App registrations → New |
| Add API Scope | App registrations → Your API → Expose an API |
| Add App Roles | App registrations → Your API → App roles |
| Add API Permission | App registrations → Your SPA → API permissions |
| Assign Roles to Users | Enterprise applications → Your API → Users and groups |

### Key URLs

| Service | URL |
|---------|-----|
| Frontend | http://localhost:4200 |
| Backend API | https://localhost:7001 |
| Azure Portal | https://portal.azure.com |
| Microsoft Entra Admin | https://entra.microsoft.com |

### Commands

```bash
# Backend
cd backend/MsalDemo.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run

# Frontend
cd frontend
npm install
npm start
```

---

*Last Updated: June 2026*
