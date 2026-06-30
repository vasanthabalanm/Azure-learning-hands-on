# Hands-On SSE Appointment Queue System (Angular + .NET + PostgreSQL)

## Objective
Build a hands-on learning project that demonstrates how to implement Server-Sent Events (SSE) for real-time updates in a healthcare appointment workflow, with clear end-to-end setup, readable code, and role-based behavior.

## Tech Stack
- Frontend: Angular (latest stable version)
- Backend: .NET Web API
- Database: PostgreSQL
- Configuration: `.env`-based connection string
- Real-time transport: Server-Sent Events (SSE)

## Functional Requirements
- Patient can book an appointment.
- Staff can map a patient/appointment to a doctor.
- Doctor can pick and examine mapped patients.
- Appointment lifecycle supports queue visibility and status transitions.
- Frontend and backend integration must be complete and runnable end-to-end.
- Include all required package installation and setup steps.

## RBAC Scope
- patient/user
- Can book appointments.
- Can view own appointment status.
- staff
- Can view incoming appointments.
- Can map patients to doctors.
- doctor
- Can view assigned and available queue.
- Can pick a patient for examination.
- Can update examination status.

## Real-Time and Concurrency Rules
- SSE must push real-time notifications when:
- Patient is mapped to a doctor.
- Doctor picks a patient from queue.
- Concurrency lock:
- If one doctor picks a patient, that same patient must immediately become unavailable to all other doctors.
- Queue consistency:
- Other doctors should see a status such as "observed by Doctor A" (or equivalent) and cannot pick that patient.
- Picked patient must be removed from selectable queue for other doctors in real time.
- Backend must enforce concurrency server-side (not frontend-only).

## Deliverables
- Source code with clear and understandable implementation:
- Angular frontend
- .NET backend API
- PostgreSQL schema, migrations, and configuration
- `.env`-driven DB connectivity
- SSE implementation including:
- Event publishing on backend
- Event subscription and handling on frontend
- Role-aware UI behavior for patient, staff, and doctor
- End-to-end setup guide including:
- Prerequisites
- Required packages and dependencies
- Backend and frontend run steps
- Connection verification steps

## Documentation Requirements
- Provide a separate markdown file dedicated to SSE that includes:
- What SSE is
- When to use SSE
- Why SSE is suitable here
- How SSE works (conceptual + project-specific flow)
- How SSE is implemented in this project (backend to frontend)
- Pros and cons
- Best-fit use cases and practical placement guidance
- Practical tips and common pitfalls
- Step-by-step teammate onboarding instructions
- This use-case document should remain concise and execution-focused.

## Non-Functional Expectations
- Code readability and maintainability are mandatory.
- Naming, structure, and comments should prioritize understandability.
- Real-time behavior should be immediate and consistent across clients.
- Role boundaries must be explicit and safe.
- Setup must be reproducible without hidden steps.

## Acceptance Criteria
- Patient can book an appointment successfully.
- Staff can map a patient to a doctor.
- Doctor A picks patient; Doctor B cannot pick the same patient.
- Doctor B sees updated status (for example, "observed by Doctor A") in near real time.
- Picked patient is removed from other doctors' selectable queue.
- SSE events are visible in UI for mapping and picking actions.
- RBAC restrictions work correctly for patient/user, staff, and doctor.
- Backend and frontend run locally with PostgreSQL via `.env` connection string.
- Documentation includes complete package/setup steps and SSE deep-dive markdown.

## Open Assumptions
- "Angular latest" means latest stable major version at implementation time.
- ".NET backend" means current LTS .NET Web API unless otherwise specified.
- Authentication mechanism is implementation-defined unless a specific standard is provided.
- SSE remains one-way server-to-client; create/update actions continue through HTTP APIs.
- Final status label text can be finalized during UI implementation while preserving required behavior.
