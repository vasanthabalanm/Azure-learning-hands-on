# Azure Communication Service POC - Healthcare Use Case

## Objective
Build POC for Azure Communication Service supporting SMS/Email/WhatsApp/Push notifications in healthcare context.

## Use Case
Trigger communications based on database records and configuration:
- Patient follow-up data fetched from DB
- Each record has associated communication channel configuration
- System triggers only enabled channels (e.g., follow-up → email+SMS only)

## Technical Requirements

**Database**: PostgreSQL (connection string in .env)

**Backend**: .NET
- SOLID principles
- Composition with dynamic polymorphism via Strategy design pattern
- No inheritance (unless explicitly confirmed)

**Infrastructure**:
- Azure Storage Explorer (if needed)
- Azure services free tier (local setup initially until final code)
- Rancher Desktop (if needed, Docker license issue)

**Reference**: Attached image shows strategy pattern code structure

## Initial Implementation Flow
1. Patient table contains follow-up data
2. Web API endpoint triggers follow-up by user
3. System determines enabled communication channels from configuration
4. Repo layer connects to DB and fetches data
5. Incremental workflow expansion thereafter

## Demo Scenarios
- **Follow-up**: SMS + Email
- **Appointment**: Email + WhatsApp

## Development Approach
Start with local setup and mock data. No Azure service creation initially—only local implementation until code completion.

**Clarification needed**: Confirm understanding before proceeding.
