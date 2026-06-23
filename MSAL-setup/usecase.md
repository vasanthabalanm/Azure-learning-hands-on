# Azure MSAL Hands-on Use Case (Corporate-Standard Starter)

## Goal
Build a small but production-style web application to learn Azure services through hands-on practice.

The application must support Azure AD multi-tenant authentication and role-based access with 3 roles.

## Tech Stack
- Frontend: Angular
- Backend: ASP.NET Core Web API with Entity Framework Core
- Database: PostgreSQL
- Identity: Microsoft Entra ID (Azure AD) with MSAL

## Required Outcome
Create an initial codebase that demonstrates:
1. Multi-tenant sign-in using Azure AD.
2. Token-based authentication from Angular to ASP.NET Core API.
3. Role-based authorization with exactly 3 roles.
4. Data persistence using PostgreSQL and EF Core.
5. Clean, readable, enterprise-style code and folder structure.

## Role Model (Initial)
- Admin: Full access to all protected endpoints.
- Manager: Access to manager + shared business endpoints.
- User: Access to basic user endpoints only.

## Functional Requirements
1. User can sign in and sign out from Angular using MSAL.
2. Application must accept users from multiple Azure AD tenants.
3. Angular must send bearer token to backend API.
4. Backend must validate JWT access token issued by Azure AD.
5. Backend must authorize endpoints based on role claims.
6. At least one endpoint per role must be implemented and tested.

## Non-Functional Requirements
1. Code must be easy to understand for new developers.
2. Project structure must follow clear separation of concerns.
3. Naming, configuration, and comments must be consistent and minimal.
4. Security basics must be applied (no secrets in source code, use config/environment variables).

## Suggested Learning Scope (Phase 1)
1. Azure AD app registrations:
	- One SPA app (Angular)
	- One API app (ASP.NET Core)
2. Configure exposed API scopes and frontend API permissions.
3. Implement role claims and role-protected endpoints.
4. Verify end-to-end authentication + authorization flow.

## Deliverable Expectation
Provide a small working sample with:
1. Angular app with MSAL setup.
2. ASP.NET Core API with JWT + role policies.
3. PostgreSQL integration via EF Core.
4. Seed or sample data.
5. README with setup and run steps.

## Clarifications Needed Before Implementation
1. Should the 3 roles be stored in Azure AD app roles only, or also in local DB?
2. Do you want local development first (localhost) and then Azure deployment later?
3. Should signup/invitation flow be included, or login-only for now?