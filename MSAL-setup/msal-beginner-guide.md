# MSAL Authentication - Beginner's Guide

A step-by-step manual for newcomers to understand how this project works. Written for developers who are new to Azure AD, MSAL, and enterprise authentication.

---

## Table of Contents

1. [What Are We Building?](#what-are-we-building)
2. [Key Concepts Explained](#key-concepts-explained)
3. [Understanding the Architecture](#understanding-the-architecture)
4. [Azure AD Setup (Visual Walkthrough)](#azure-ad-setup-visual-walkthrough)
5. [Backend Deep Dive](#backend-deep-dive)
6. [Frontend Deep Dive](#frontend-deep-dive)
7. [How Authentication Works (Flow)](#how-authentication-works-flow)
8. [Connecting All the Pieces](#connecting-all-the-pieces)
9. [Package Reference](#package-reference)
10. [Glossary](#glossary)

---

## What Are We Building?

We're building a **secure web application** where:

1. Users log in with their **Microsoft work account** (not personal Gmail/Yahoo)
2. Users are assigned **roles** (Admin, Manager, User)
3. Different pages are accessible based on roles
4. Backend API only responds to authenticated users with correct roles

### Real-World Example

Think of it like an office building:
- **Login** = Security guard checks your ID card
- **Roles** = Your access level (CEO can go anywhere, intern has limited access)
- **Pages** = Different floors/rooms
- **API** = The work you can do in each room

---

## Key Concepts Explained

### What is Azure AD (Microsoft Entra ID)?

Azure AD is Microsoft's cloud-based identity service. It's like a central database of:
- All employees in your organization
- Their login credentials
- What applications they can access

**Analogy**: It's the HR department that knows who works at the company and what they're allowed to do.

### What is MSAL?

**MSAL** = Microsoft Authentication Library

It's a code library that handles:
- Showing the Microsoft login popup
- Getting tokens (digital passes) after login
- Attaching tokens to API requests

**Frontend uses**: `@azure/msal-browser` and `@azure/msal-angular`
**Backend uses**: `Microsoft.Identity.Web`

### What is a Token?

A **token** is like a digital pass that proves:
1. Who you are (identity)
2. What you're allowed to do (permissions/roles)

When you log in, Azure AD gives you a token. You show this token to the API to prove you're allowed to access it.

### What is OAuth 2.0?

OAuth 2.0 is a standard protocol (set of rules) for authorization. Think of it like:

> "I give my house key to a trusted friend (app) so they can water my plants (access specific data) while I'm away, but they can't throw parties (access other things)."

### Client ID vs Tenant ID

| Term | What It Is | Example |
|------|------------|---------|
| **Tenant ID** | Your organization's unique ID in Azure | `25680fff-d55e-4bfe-b621-0958708d1098` |
| **Client ID** | Your application's unique ID | `de265c95-9f62-4465-8caf-6a4cd974830c` |

**Analogy**: 
- Tenant ID = Your company's address
- Client ID = Your employee ID badge number

---

## Understanding the Architecture

### Three Main Parts

```
┌─────────────────────────────────────────────────────────────────┐
│                        USER'S BROWSER                           │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │            ANGULAR FRONTEND (Port 4200)                  │   │
│  │  - Displays UI (Login button, Dashboards)                │   │
│  │  - Uses MSAL to handle Microsoft login                   │   │
│  │  - Sends requests to Backend API with token              │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP Requests (with Token)
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│            ASP.NET CORE BACKEND API (Port 7001)                 │
│  - Validates tokens from Azure AD                               │
│  - Checks user roles                                            │
│  - Returns data if authorized                                   │
│  - Connects to database                                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Database Queries
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                  POSTGRESQL DATABASE (Port 5432)                │
│  - Stores user profiles                                         │
│  - Stores audit logs                                            │
│  - Sample data for testing                                      │
└─────────────────────────────────────────────────────────────────┘
                              
                              ▲
                              │ Token Validation
                              │
┌─────────────────────────────────────────────────────────────────┐
│                    MICROSOFT ENTRA ID (Azure AD)                │
│  - Authenticates users (checks username/password)              │
│  - Issues tokens                                                │
│  - Stores role assignments                                      │
└─────────────────────────────────────────────────────────────────┘
```

### Why Two App Registrations?

| App | Purpose | Why Separate? |
|-----|---------|---------------|
| **MsalDemo-SPA** | Frontend app identity | Handles user login, redirect back to browser |
| **MsalDemo-API** | Backend app identity | Validates tokens, defines roles |

**Think of it like**: A restaurant needs two things:
- A **reception desk** (SPA) to welcome customers
- A **kitchen** (API) that verifies orders and prepares food

Both are part of the same restaurant but have different responsibilities.

---

## Azure AD Setup (Visual Walkthrough)

### Step 1: Access Azure Portal

1. Go to https://portal.azure.com
2. Search for "Microsoft Entra ID" (formerly Azure Active Directory)
3. Click on "App registrations" in the left menu

### Step 2: Create API App Registration

```
New Registration Form:
┌─────────────────────────────────────────────────────────────┐
│ Name: MsalDemo-API                                          │
│                                                             │
│ Supported account types:                                    │
│ ○ Single tenant                                             │
│ ● Multi-tenant (accounts in any org)  ◄── SELECT THIS      │
│ ○ Personal Microsoft accounts                               │
│                                                             │
│ Redirect URI: (leave blank)                                 │
│                                                             │
│ [Register]                                                  │
└─────────────────────────────────────────────────────────────┘
```

After creation, you'll see:
```
Overview Page:
┌─────────────────────────────────────────────────────────────┐
│ Application (client) ID: 171d9cb6-72eb-4a41-b83c-...       │ ◄── COPY THIS
│ Directory (tenant) ID:   25680fff-d55e-4bfe-b621-...       │ ◄── COPY THIS
└─────────────────────────────────────────────────────────────┘
```

### Step 3: Expose an API (Create Scope)

Navigate to "Expose an API":

```
┌─────────────────────────────────────────────────────────────┐
│ Application ID URI: api://171d9cb6-72eb-4a41-b83c-...      │
│ [Set] ◄── Click this first                                 │
├─────────────────────────────────────────────────────────────┤
│ Scopes defined by this API:                                 │
│                                                             │
│ [+ Add a scope]                                             │
│                                                             │
│ Scope name: access_as_user                                  │
│ Who can consent: Admins and users                           │
│ Admin consent display name: Access MsalDemo API             │
│ Admin consent description: Allow access to API              │
│ State: Enabled                                              │
└─────────────────────────────────────────────────────────────┘
```

**What is a Scope?**

A scope defines what the app can do. `access_as_user` means "access the API on behalf of the logged-in user".

### Step 4: Create App Roles

Navigate to "App roles" → "Create app role":

```
Create App Role Form (repeat 3 times):
┌─────────────────────────────────────────────────────────────┐
│ Display name: Admin                                         │
│ Allowed member types: ● Users/Groups                        │
│ Value: Admin  ◄── This is what appears in the token         │
│ Description: Full administrative access                     │
│ [Apply]                                                     │
└─────────────────────────────────────────────────────────────┘
```

Create three roles: **Admin**, **Manager**, **User**

### Step 5: Create SPA App Registration

Similar to API, but with redirect URI:

```
New Registration Form:
┌─────────────────────────────────────────────────────────────┐
│ Name: MsalDemo-SPA                                          │
│                                                             │
│ Redirect URI:                                               │
│ Platform: Single-page application (SPA) ◄── SELECT THIS    │
│ URL: http://localhost:4200                                  │
│                                                             │
│ [Register]                                                  │
└─────────────────────────────────────────────────────────────┘
```

### Step 6: Add API Permissions to SPA

Navigate to "API permissions" in SPA app:

```
┌─────────────────────────────────────────────────────────────┐
│ [+ Add a permission]                                        │
│                                                             │
│ Select: My APIs → MsalDemo-API                              │
│ Check: ☑ access_as_user                                     │
│                                                             │
│ [Add permissions]                                           │
│                                                             │
│ [Grant admin consent for...] ◄── Click this!               │
└─────────────────────────────────────────────────────────────┘
```

### Step 7: Assign Roles to Users

**IMPORTANT**: Go to "Enterprise applications" (NOT App registrations!)

```
Enterprise Applications → MsalDemo-API → Users and groups

┌─────────────────────────────────────────────────────────────┐
│ [+ Add user/group]                                          │
│                                                             │
│ User: balan1@yourcompany.onmicrosoft.com                   │
│ Role: Admin                                                 │
│                                                             │
│ [Assign]                                                    │
└─────────────────────────────────────────────────────────────┘
```

### ⚠️ Common Problem: "I Can't Login with My Account!"

This is the #1 issue beginners face. Here's why and how to fix it:

**The Problem**:
```
You created Azure with: balanm@gmail.com (personal account)
                        ↓
         This account lives in Tenant A
                        
Your app is registered in: yourcompany.onmicrosoft.com
                        ↓
         This is Tenant B

❌ Tenant A ≠ Tenant B = LOGIN FAILS!
```

**Why This Happens**:
- When you sign up for Azure with a personal email (Gmail, Outlook, etc.), Microsoft creates a "guest" tenant for you
- When you create an organization (like `yourcompany.onmicrosoft.com`), that's a DIFFERENT tenant
- Your personal account is NOT automatically a member of your organization tenant

**The Fix - Create a User in Your Organization**:

1. Go to **Microsoft Entra ID** → **Users** → **New user**

2. Select **"Create new user"**

3. Fill in:
   ```
   ┌─────────────────────────────────────────────────────────────┐
   │ User principal name: balan1                                 │
   │ Domain: @yourcompany.onmicrosoft.com  ◄── MUST match tenant │
   │                                                             │
   │ Display name: Balan Test User                               │
   │                                                             │
   │ Password: [Auto-generate] or [Let me create]                │
   │                                                             │
   │ [Create]                                                    │
   └─────────────────────────────────────────────────────────────┘
   ```

4. **Copy the temporary password** (you'll need it for first login)

5. Now assign roles to this new user (Step 7 above)

6. Login with: `balan1@yourcompany.onmicrosoft.com`

**Visual Explanation**:
```
BEFORE (Wrong):
┌──────────────────┐     ┌──────────────────┐
│ Your Gmail       │     │ Your Org Tenant  │
│ balanm@gmail.com │  ✗  │ yourcompany....  │
│ (Tenant A)       │     │ (Tenant B)       │
└──────────────────┘     │                  │
                         │ App Registration │
                         │ (lives here)     │
                         └──────────────────┘

AFTER (Correct):
┌──────────────────┐
│ Your Org Tenant  │
│ yourcompany....  │
│ (Tenant B)       │
│                  │
│ ✓ balan1@...     │ ◄── User INSIDE the tenant
│ ✓ App Registration│ ◄── App INSIDE the tenant
└──────────────────┘
```

**Key Takeaway**: 
> The user logging in MUST belong to the same Azure AD tenant where your app is registered. Create test users inside your organization tenant!

---

## Backend Deep Dive

### Packages/Libraries Used

Install these NuGet packages:

```xml
<!-- MsalDemo.Api.csproj -->
<PackageReference Include="Microsoft.Identity.Web" Version="2.17.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

| Package | Purpose |
|---------|---------|
| `Microsoft.Identity.Web` | Validates Azure AD tokens, extracts claims |
| `JwtBearer` | Handles JWT (JSON Web Token) authentication |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Connects to PostgreSQL database |

### Understanding Program.cs

```csharp
// 1. AUTHENTICATION - Tell the app how to validate tokens
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
```

**What this does**:
- When a request comes in with a token, validate it against Azure AD
- Check if the token is from our registered API
- Extract user information from the token

```csharp
// 2. AUTHORIZATION - Define what roles can do what
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new RolesRequirement("Admin")));
    
    options.AddPolicy("ManagerOrAbove", policy =>
        policy.Requirements.Add(new RolesRequirement("Admin", "Manager")));
});
```

**What this does**:
- Creates named policies that controllers can use
- "AdminOnly" = only users with Admin role
- "ManagerOrAbove" = users with Admin OR Manager role

```csharp
// 3. CORS - Allow frontend to call backend
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

**What this does**:
- By default, browsers block requests from one domain to another
- CORS allows `localhost:4200` (frontend) to call `localhost:7001` (backend)

### Understanding Controllers

```csharp
[ApiController]
[Route("api/admin")]
[Authorize(Policy = Policies.AdminOnly)]  // ◄── Only Admins can access
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public AdminController(AppDbContext context)
    {
        _context = context;  // Database connection injected
    }
    
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        // Fetch from database
        var users = await _context.UserProfiles.ToListAsync();
        return Ok(users);
    }
}
```

**What happens when someone calls `/api/admin/users`**:

1. Request arrives with token in header: `Authorization: Bearer eyJ0eX...`
2. `[Authorize]` attribute triggers token validation
3. `Policy = AdminOnly` checks if user has "Admin" role
4. If authorized, `GetAllUsers()` runs and fetches from database
5. Data is returned as JSON

### Database Connection

```csharp
// Data/AppDbContext.cs
public class AppDbContext : DbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure table names, relationships, etc.
    }
}
```

**Entity Framework Core (EF Core)**:
- ORM (Object-Relational Mapper) - converts C# objects to database tables
- `DbSet<UserProfile>` = a table of UserProfile records
- No need to write raw SQL queries

---

## Frontend Deep Dive

### Packages/Libraries Used

```json
// package.json
{
  "dependencies": {
    "@angular/core": "^18.0.0",
    "@azure/msal-angular": "^3.0.0",      // Angular-specific MSAL
    "@azure/msal-browser": "^3.0.0",      // Core MSAL library
    "rxjs": "^7.8.0"                       // Reactive programming
  }
}
```

| Package | Purpose |
|---------|---------|
| `@azure/msal-browser` | Handles login popup, token storage, token refresh |
| `@azure/msal-angular` | Angular integration (guards, interceptors, services) |
| `rxjs` | Handle async operations (Observables) |

### Understanding main.ts

```typescript
// 1. Create MSAL instance - the "engine" for authentication
export function MSALInstanceFactory(): PublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: 'de265c95-9f62-4465-8caf-6a4cd974830c',  // SPA Client ID
      authority: 'https://login.microsoftonline.com/common',
      redirectUri: 'http://localhost:4200'
    },
    cache: {
      cacheLocation: 'localStorage'  // Where to store tokens
    }
  });
}
```

**What each config means**:
- `clientId`: Your SPA app's ID from Azure
- `authority`: Where to send users to log in (Microsoft's login page)
- `redirectUri`: Where Microsoft sends users after login
- `cacheLocation`: Store tokens in browser's localStorage

```typescript
// 2. Configure which API endpoints need tokens
export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap: new Map([
      // Any request to this URL will automatically include the token
      ['https://localhost:7001/api', ['api://171d9cb6-.../access_as_user']]
    ])
  };
}
```

**What this does**:
- Any HTTP request to `localhost:7001/api` will automatically have the token attached
- The token will include the `access_as_user` scope

### Understanding app.component.ts

```typescript
export class AppComponent implements OnInit {
  isLoggedIn = false;
  
  constructor(
    private authService: MsalService,
    private msalBroadcastService: MsalBroadcastService
  ) {}
  
  ngOnInit(): void {
    // Listen for login events
    this.msalBroadcastService.msalSubject$.pipe(
      filter(msg => msg.eventType === EventType.LOGIN_SUCCESS)
    ).subscribe(() => {
      // User just logged in!
      this.handleLoginSuccess();
    });
  }
  
  login(): void {
    // Redirect to Microsoft login page
    this.authService.loginRedirect();
  }
  
  logout(): void {
    // Clear session and redirect to Microsoft logout
    this.authService.logoutRedirect();
  }
}
```

### Understanding role.guard.ts

Guards protect routes (pages) based on conditions.

```typescript
@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  
  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    // 1. Wait for MSAL to be ready
    return this.msalBroadcastService.inProgress$.pipe(
      filter(status => status === InteractionStatus.None),
      take(1),
      switchMap(() => {
        // 2. Get user's roles from API
        return this.roleService.getUserRoles();
      }),
      map(userRoles => {
        // 3. Check if user has required role
        const requiredRoles = route.data['roles'] as string[];
        return requiredRoles.some(role => userRoles.includes(role));
      })
    );
  }
}
```

**In routes**:
```typescript
{
  path: 'admin',
  component: AdminComponent,
  canActivate: [MsalGuard, RoleGuard],  // Must pass both guards
  data: { roles: ['Admin'] }             // Required roles
}
```

### Understanding role.service.ts

```typescript
@Injectable({ providedIn: 'root' })
export class RoleService {
  private apiUrl = 'https://localhost:7001/api';
  
  constructor(private http: HttpClient) {}
  
  getUserRoles(): Observable<string[]> {
    // Call backend API - token is automatically attached by MsalInterceptor
    return this.http.get<any>(`${this.apiUrl}/me`).pipe(
      map(response => response.roles || [])
    );
  }
}
```

**Flow**:
1. Frontend calls `/api/me`
2. MsalInterceptor automatically adds token to request header
3. Backend validates token, extracts roles from claims
4. Backend returns roles to frontend
5. Frontend uses roles for navigation decisions

---

## How Authentication Works (Flow)

### Complete Login Flow (Step by Step)

```
USER                    FRONTEND              AZURE AD              BACKEND
  │                        │                     │                     │
  │  1. Click Login        │                     │                     │
  │───────────────────────▶│                     │                     │
  │                        │                     │                     │
  │                        │  2. Redirect to     │                     │
  │                        │     login page      │                     │
  │                        │────────────────────▶│                     │
  │                        │                     │                     │
  │  3. Enter credentials  │                     │                     │
  │───────────────────────────────────────────────▶│                   │
  │                        │                     │                     │
  │                        │  4. Redirect with   │                     │
  │  ◀───────────────────────────────────────────│                     │
  │        ID token &      │                     │                     │
  │        Auth code       │                     │                     │
  │                        │                     │                     │
  │                        │  5. Exchange code   │                     │
  │                        │     for access      │                     │
  │                        │     token           │                     │
  │                        │────────────────────▶│                     │
  │                        │                     │                     │
  │                        │  6. Access token    │                     │
  │                        │◀────────────────────│                     │
  │                        │                     │                     │
  │                        │  7. Call API with   │                     │
  │                        │     access token    │                     │
  │                        │───────────────────────────────────────────▶│
  │                        │                     │                     │
  │                        │                     │  8. Validate token  │
  │                        │                     │◀────────────────────│
  │                        │                     │     (is it valid?)  │
  │                        │                     │                     │
  │                        │                     │  9. Token valid     │
  │                        │                     │────────────────────▶│
  │                        │                     │                     │
  │                        │ 10. Return data     │                     │
  │                        │◀──────────────────────────────────────────│
  │                        │                     │                     │
  │ 11. Show dashboard     │                     │                     │
  │◀───────────────────────│                     │                     │
```

### What's in a Token?

When decoded, an access token looks like:

```json
{
  "header": {
    "alg": "RS256",
    "typ": "JWT"
  },
  "payload": {
    "aud": "api://171d9cb6-72eb-4a41-b83c-14d4807c66c5",
    "iss": "https://login.microsoftonline.com/25680fff-.../v2.0",
    "name": "Balan M",
    "preferred_username": "balan1@company.onmicrosoft.com",
    "roles": ["Admin"],
    "exp": 1719158400
  },
  "signature": "abc123..."
}
```

| Field | Meaning |
|-------|---------|
| `aud` | Audience - which API this token is for |
| `iss` | Issuer - who created this token (Azure AD) |
| `name` | User's display name |
| `roles` | User's assigned roles |
| `exp` | Expiration time (Unix timestamp) |

---

## Connecting All the Pieces

### Configuration Values That Must Match

```
┌─────────────────────────────────────────────────────────────────┐
│                     AZURE AD PORTAL                             │
│                                                                 │
│  API App Registration:                                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Client ID: 171d9cb6-72eb-4a41-b83c-14d4807c66c5        │───┐│
│  │ App ID URI: api://171d9cb6-72eb-4a41-b83c-14d4807c66c5 │───┤│
│  │ Scope: access_as_user                                   │───┤│
│  └─────────────────────────────────────────────────────────┘   ││
│                                                                 ││
│  SPA App Registration:                                          ││
│  ┌─────────────────────────────────────────────────────────┐   ││
│  │ Client ID: de265c95-9f62-4465-8caf-6a4cd974830c        │───┤│
│  │ Redirect URI: http://localhost:4200                     │───┤│
│  └─────────────────────────────────────────────────────────┘   ││
└─────────────────────────────────────────────────────────────────┘│
                                                                   │
┌────────────────────────┬─────────────────────────────────────────┘
│                        │
▼                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                     BACKEND CONFIG                              │
│  appsettings.Development.json                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ "ClientId": "171d9cb6-72eb-4a41-b83c-14d4807c66c5"     │◀──┘
│  │ "Audience": "api://171d9cb6-72eb-4a41-b83c-14d4807c66c5"│
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                     FRONTEND CONFIG                             │
│  environment.ts                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ clientId: "de265c95-9f62-4465-8caf-6a4cd974830c"       │◀──┘
│  │ redirectUri: "http://localhost:4200"                    │
│  │ scopes: ["api://171d9cb6-.../access_as_user"]          │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### How HTTP Interceptor Attaches Token

```typescript
// When you write this in your component:
this.http.get('https://localhost:7001/api/admin/users')

// MsalInterceptor automatically transforms it to:
// Headers: {
//   "Authorization": "Bearer eyJ0eXAiOiJKV1QiLCJhbGc..."
// }
```

### How Backend Validates Token

```csharp
// 1. Token arrives in request header
// Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGc...

// 2. Microsoft.Identity.Web validates:
//    - Is the signature valid? (not tampered)
//    - Is it expired?
//    - Is the audience correct? (meant for our API)
//    - Is the issuer correct? (from Azure AD)

// 3. If valid, User object is populated with claims
// User.FindFirst("name")?.Value → "Balan M"
// User.FindAll("roles") → ["Admin"]

// 4. Authorization handler checks roles
// Does user have "Admin" role? → Yes → Allow access
```

---

## Package Reference

### Backend (NuGet Packages)

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Identity.Web | 2.17.0 | Azure AD token validation |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT authentication middleware |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.0 | PostgreSQL database provider |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | EF Core CLI tools (migrations) |

**Install Command**:
```bash
dotnet add package Microsoft.Identity.Web
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Frontend (npm Packages)

| Package | Version | Purpose |
|---------|---------|---------|
| @azure/msal-browser | 3.x | Core MSAL library |
| @azure/msal-angular | 3.x | Angular integration |
| @angular/core | 18.x | Angular framework |
| rxjs | 7.x | Reactive extensions |

**Install Command**:
```bash
npm install @azure/msal-browser @azure/msal-angular
```

---

## Glossary

| Term | Definition |
|------|------------|
| **Access Token** | A digital pass that grants access to protected APIs |
| **App Registration** | Registering your app in Azure AD to enable authentication |
| **Authorization** | Checking if a user is ALLOWED to do something (after authentication) |
| **Authentication** | Verifying WHO the user is (login process) |
| **Claim** | A piece of information about the user in the token (name, email, roles) |
| **Client ID** | Unique identifier for your app in Azure AD |
| **CORS** | Cross-Origin Resource Sharing - allows browser to call different domain |
| **EF Core** | Entity Framework Core - .NET ORM for database operations |
| **Guard** | Angular mechanism to protect routes |
| **Interceptor** | Middleware that modifies HTTP requests/responses |
| **JWT** | JSON Web Token - the format for access tokens |
| **MSAL** | Microsoft Authentication Library |
| **OAuth 2.0** | Industry standard protocol for authorization |
| **Scope** | Permission that defines what access a token grants |
| **Tenant** | An organization in Azure AD (your company) |
| **Token** | Digital pass containing user identity and permissions |

---

## Quick Start Commands

```bash
# 1. Start PostgreSQL (if not running)
# Windows: Start PostgreSQL service in Services

# 2. Create database
psql -U postgres -c "CREATE DATABASE msal_demo_dev;"

# 3. Start Backend
cd MSAL-setup/backend/MsalDemo.Api
dotnet ef database update
dotnet run

# 4. Start Frontend (new terminal)
cd MSAL-setup/frontend
npm install
npm start

# 5. Open browser
# Go to http://localhost:4200
# Click "Sign In with Microsoft"
```

---

## Summary

What we built:
1. **Azure AD** stores users and roles
2. **Frontend (Angular)** handles login UI and sends authenticated requests
3. **Backend (ASP.NET Core)** validates tokens and protects API endpoints
4. **Database (PostgreSQL)** stores application data

The key insight: **tokens are like digital ID cards**. Azure AD issues them, the frontend carries them, and the backend checks them before allowing access.

---

*Last Updated: June 2026*
