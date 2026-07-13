# SSE Appointment Queue System - Project Summary

## Overview
A **hands-on learning project** demonstrating **Server-Sent Events (SSE)** for real-time updates in a healthcare appointment queue system.

## Tech Stack
- **Frontend:** Angular 17 (standalone components)
- **Backend:** .NET 8 Web API
- **Real-time:** Server-Sent Events (SSE)
- **Storage:** In-memory (no database needed for demo)

## What This Demo Covers

### ✅ Core SSE Concepts
- ✅ Server-to-client real-time push
- ✅ EventSource API (browser native)
- ✅ Broadcasting events to multiple clients
- ✅ Auto-reconnection handling
- ✅ SSE message format (`data: {json}\n\n`)

### ✅ Real-World Scenarios
- ✅ Patient books appointment → **SSE notifies staff**
- ✅ Staff maps patient to doctor → **SSE notifies doctor**
- ✅ Doctor picks patient → **SSE updates ALL other doctors' queues**
- ✅ Concurrency control (server-side lock prevents race conditions)

### ✅ Role-Based Views
- **Patient:** Book appointments, view status
- **Staff:** View all appointments, map to doctors
- **Doctor:** View assigned queue, pick patients for examination

---

## Key Files

### Backend (.NET)
```
backend/AppointmentQueueAPI/
├── Controllers/
│   ├── SseController.cs        # 🔥 SSE endpoint (/api/sse/stream)
│   └── AppointmentController.cs # Business logic (book, map, pick)
├── Services/
│   ├── SseService.cs           # 🔥 Broadcasts events to all clients
│   └── AppointmentService.cs   # 🔒 Concurrency lock for picking
└── Program.cs                  # CORS + DI setup
```

### Frontend (Angular)
```
frontend/appointment-queue-ui/src/app/
├── services/
│   ├── sse.service.ts          # 🔥 EventSource wrapper
│   └── appointment.service.ts  # HTTP API calls
└── app.component.ts            # 🔥 Single-page app (all roles in one component)
```

---

## How SSE Works in This Project

### 1. Connection Established
```typescript
// Frontend (Angular)
this.eventSource = new EventSource('http://localhost:5000/api/sse/stream');
```

```csharp
// Backend (.NET)
[HttpGet("stream")]
public async Task StreamEvents()
{
    Response.ContentType = "text/event-stream"; // SSE header
    var writer = new StreamWriter(Response.Body);
    _sseService.AddClient(writer); // Add to active connections
    await Request.HttpContext.RequestAborted.WaitHandle.WaitOneAsync();
}
```

### 2. Event Broadcasting
```csharp
// Backend: When doctor picks patient
[HttpPost("pick")]
public async Task<IActionResult> PickPatient(...)
{
    var appointment = _appointmentService.PickPatient(...);
    await _sseService.BroadcastEvent("patient_picked", appointment); // 🔥 Send to ALL
    return Ok(appointment);
}
```

### 3. Frontend Receives Event
```typescript
// Frontend: Auto-updates UI
this.sseService.events$.subscribe(event => {
    if (event.eventType === 'patient_picked') {
        this.refreshQueue(); // Real-time update!
    }
});
```

---

## Concurrency Control

### The Challenge
**Two doctors see the same patient. Both click "Pick" simultaneously.**

### The Solution
```csharp
private readonly object _pickLock = new(); // 🔒 Thread-safe lock

public Appointment? PickPatient(int appointmentId, string doctorName)
{
    lock (_pickLock)
    {
        if (appointment.Status == "MappedToDoctor")
        {
            appointment.Status = "PickedByDoctor";
            await _sseService.BroadcastEvent("patient_picked", appointment);
            return appointment;
        }
        return null; // Already picked
    }
}
```

**Result:**
- Only **one doctor** successfully picks the patient
- All other doctors receive **instant SSE update** showing patient is unavailable
- No race conditions ✅

---

## Quick Start

### Backend
```bash
cd backend/AppointmentQueueAPI
dotnet restore
dotnet run
# Runs on http://localhost:5000
```

### Frontend
```bash
cd frontend/appointment-queue-ui
npm install
npm start
# Runs on http://localhost:4200
```

### Test SSE
1. Open **two browser tabs** (both at http://localhost:4200)
2. **Tab 1:** Switch to "Doctor" role, enter "DrSmith"
3. **Tab 2:** Switch to "Doctor" role, enter "DrJones"
4. **Tab 3:** Switch to "Staff" role
   - Book a patient
   - Map to "DrSmith"
5. **Watch Tab 1:** Patient appears in queue (SSE event!)
6. **Click "Pick Patient" in Tab 1**
7. **Watch Tab 2:** Patient immediately disappears (SSE event!)

---

## Learning Path

### Phase 1: Concepts (30 min)
Read **SSE-DOCUMENTATION.md** sections:
- What is SSE?
- When to use SSE?
- How SSE works (conceptual flow)

### Phase 2: Setup (1 hour)
Follow **SETUP.md**:
- Install prerequisites
- Run backend and frontend
- Verify SSE connection

### Phase 3: Code Exploration (2 hours)
**Backend:**
1. `SseController.cs:14` - See how SSE connection is opened
2. `SseService.cs:17` - See how events are broadcasted
3. `AppointmentController.cs:51` - See where events are triggered

**Frontend:**
1. `sse.service.ts:22` - See EventSource usage
2. `app.component.ts:183` - See event subscription
3. Open DevTools → Network → EventSource tab

### Phase 4: Testing (1 hour)
- Book appointments
- Map to doctors
- Pick patients (with multiple browsers)
- Watch SSE events in console and DevTools

### Phase 5: Extend (Optional)
- Add new event type
- Implement heartbeat/keep-alive
- Add authentication
- Connect to PostgreSQL

---

## Why This Demo is Great for Learning

### ✅ Minimal Complexity
- No database setup required
- No authentication to configure
- Single-page Angular app (no routing)
- Clean, commented code

### ✅ Covers Real Scenarios
- Multi-client updates
- Concurrency handling
- Role-based behavior
- Error handling

### ✅ Interactive Testing
- See SSE in action immediately
- Open DevTools to inspect events
- Test with multiple browsers
- Understand race conditions

### ✅ Production-Ready Patterns
- Proper CORS setup
- Thread-safe concurrency
- Graceful disconnection handling
- Event-driven architecture

---

## SSE Message Format

**Backend sends:**
```
data: {"eventType":"patient_picked","data":{...},"timestamp":"2026-07-08T10:30:00Z"}

```
*(Note: Two newlines mark end of event)*

**Frontend receives:**
```typescript
{
  eventType: "patient_picked",
  data: { id: 1, patientName: "John", status: "PickedByDoctor", ... },
  timestamp: "2026-07-08T10:30:00Z"
}
```

---

## Event Types in This Demo

| Event Type           | Triggered When            | Recipients    |
|---------------------|---------------------------|---------------|
| `connected`         | Client connects to SSE    | Single client |
| `appointment_booked`| Patient books appointment | All clients   |
| `patient_mapped`    | Staff maps to doctor      | All clients   |
| `patient_picked`    | Doctor picks patient      | All clients   |

---

## Architecture Diagram

```
┌──────────────┐
│   Browser 1  │────┐
│   (Angular)  │    │
└──────────────┘    │
                    │ SSE /api/sse/stream
┌──────────────┐    │ (Keep-Alive HTTP)
│   Browser 2  │────┤
│   (Angular)  │    │
└──────────────┘    │
                    ▼
            ┌────────────────┐
            │  .NET Web API  │
            │                │
            │  SseService    │─── Broadcasts to all
            │  (In-memory)   │
            └────────────────┘
```

---

## Production Considerations

### For Production Use:
1. **Database:** Add PostgreSQL/SQL Server
2. **Authentication:** JWT tokens for SSE connections
3. **Scaling:** Redis Pub/Sub for multi-server broadcasting
4. **Monitoring:** Track active connections, disconnection rates
5. **Heartbeat:** Send keep-alive messages every 30 seconds

### Example Redis Scaling:
```csharp
// Server A publishes
await _redis.PublishAsync("sse-events", message);

// All servers subscribe
_redis.Subscribe("sse-events", (msg) => {
    _sseService.BroadcastEvent(msg);
});
```

---

## Resources

- **SSE Documentation:** `SSE-DOCUMENTATION.md` (deep dive)
- **Setup Guide:** `SETUP.md` (step-by-step)
- **MDN EventSource:** https://developer.mozilla.org/en-US/docs/Web/API/EventSource
- **SSE Spec:** https://html.spec.whatwg.org/multipage/server-sent-events.html

---

## Success Criteria ✅

After completing this demo, you should be able to:

✅ Explain what SSE is and when to use it  
✅ Implement SSE endpoint in .NET  
✅ Connect to SSE using EventSource API  
✅ Broadcast events to multiple clients  
✅ Handle concurrency with server-side locks  
✅ Debug SSE connections using DevTools  
✅ Differentiate SSE from WebSockets  

---

## Next Steps

1. ✅ Run the demo (follow SETUP.md)
2. ✅ Read SSE-DOCUMENTATION.md
3. ✅ Test with multiple browsers
4. ✅ Explore the code
5. ✅ Add a new event type
6. ✅ Consider adding PostgreSQL
7. ✅ Implement authentication

**Ready to start? Run the backend, then the frontend, and open http://localhost:4200!** 🚀
