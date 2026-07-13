# Server-Sent Events (SSE) - Complete Guide

## What is SSE?

**Server-Sent Events (SSE)** is a server push technology that enables a server to send real-time updates to clients over a single, long-lived HTTP connection. Unlike WebSockets (which are bidirectional), SSE provides **one-way communication from server to client**.

### Key Characteristics:
- **Built on HTTP**: Uses standard HTTP protocol
- **Unidirectional**: Server → Client only
- **Auto-reconnection**: Browser automatically reconnects if connection drops
- **Text-based**: Sends data as UTF-8 text
- **Event-driven**: Clients subscribe to events and react when they arrive

---

## When to Use SSE?

### ✅ **Perfect Use Cases:**
1. **Real-time notifications** (new message, status updates)
2. **Live dashboards** (stock tickers, monitoring metrics)
3. **Progress tracking** (file uploads, job processing)
4. **Activity feeds** (social media updates, news feeds)
5. **Queue systems** (appointment queues, order tracking)
6. **Live scores** (sports, gaming leaderboards)

### ❌ **NOT Suitable For:**
1. **Bidirectional communication** (use WebSockets instead)
2. **Binary data streaming** (SSE is text-only)
3. **High-frequency updates** (>1000 messages/sec - WebSocket is better)
4. **Chat applications** (need client→server real-time too)

---

## Why SSE is Suitable for This Project?

Our **Appointment Queue System** is a **perfect SSE use case** because:

### 1. **Server-Initiated Updates**
- When staff maps a patient to a doctor → **notify the doctor**
- When a doctor picks a patient → **notify all other doctors**
- When a patient is booked → **notify staff**

### 2. **One-Way Data Flow**
- Actions (book, map, pick) happen via **HTTP POST** APIs
- Updates flow **server → clients** via **SSE**
- No need for client→server real-time (SSE perfect fit)

### 3. **Multiple Concurrent Clients**
- Multiple doctors viewing the same queue
- Each needs **instant updates** when queue changes
- SSE broadcasts to all connected clients

### 4. **Concurrency Control**
- When Doctor A picks a patient, Doctor B must see it **immediately**
- SSE ensures **real-time queue consistency** across all clients

---

## How SSE Works (Conceptual Flow)

```
┌─────────────┐                          ┌─────────────┐
│   Client 1  │                          │   Server    │
│   (Doctor)  │                          │             │
└──────┬──────┘                          └──────┬──────┘
       │                                        │
       │  1. GET /api/sse/stream               │
       │──────────────────────────────────────>│
       │                                        │
       │  2. HTTP 200 (Keep-Alive)             │
       │     Content-Type: text/event-stream   │
       │<──────────────────────────────────────│
       │                                        │
       │  3. Connection stays open...          │
       │━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━│
       │                                        │
┌──────▼──────┐                          │
│   Client 2  │                          │
│   (Doctor)  │                          │
└──────┬──────┘                          │
       │  GET /api/sse/stream               │
       │──────────────────────────────────────>│
       │<──────────────────────────────────────│
       │━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━│
       │                                        │
       │                                   ┌────▼────┐
       │                                   │  Event  │
       │                                   │ Happens │
       │                                   │(Patient │
       │                                   │ Picked) │
       │                                   └────┬────┘
       │                                        │
       │  data: {"eventType":"patient_picked"} │
       │<──────────────────────────────────────│
       │<──────────────────────────────────────│ (Broadcast to ALL)
       │                                        │
       │  4. Both clients receive instantly!   │
       │                                        │
```

---

## How SSE Works in This Project

### Backend Implementation (.NET)

#### 1. **SSE Controller** (`SseController.cs`)
```csharp
[HttpGet("stream")]
public async Task StreamEvents()
{
    // Set SSE-specific headers
    Response.ContentType = "text/event-stream";
    Response.Headers.Add("Cache-Control", "no-cache");
    Response.Headers.Add("Connection", "keep-alive");

    var writer = new StreamWriter(Response.Body);
    _sseService.AddClient(writer);

    // Keep connection open until client disconnects
    await Request.HttpContext.RequestAborted.WaitHandle.WaitOneAsync();
}
```

**Key Points:**
- Sets `Content-Type: text/event-stream` (required for SSE)
- Keeps HTTP connection open indefinitely
- Adds client to active connections list
- Cleans up on disconnect

#### 2. **SSE Service** (`SseService.cs`)
```csharp
public async Task BroadcastEvent(string eventType, object data)
{
    var message = JsonSerializer.Serialize(new
    {
        eventType,
        data,
        timestamp = DateTime.UtcNow
    });

    // Send to ALL connected clients
    foreach (var client in _clients)
    {
        await client.WriteAsync($"data: {message}\n\n");
        await client.FlushAsync();
    }
}
```

**SSE Message Format:**
```
data: {"eventType":"patient_picked","data":{...},"timestamp":"..."}

```
*(Note: Two newlines `\n\n` signal end of event)*

#### 3. **Broadcasting Events** (`AppointmentController.cs`)
```csharp
[HttpPost("pick")]
public async Task<IActionResult> PickPatient([FromBody] PickPatientRequest request)
{
    var appointment = _appointmentService.PickPatient(...);

    // 🔥 THIS IS THE MAGIC - Broadcast to all clients
    await _sseService.BroadcastEvent("patient_picked", appointment);

    return Ok(appointment);
}
```

---

### Frontend Implementation (Angular)

#### 1. **SSE Service** (`sse.service.ts`)
```typescript
connect(url: string): void {
    // EventSource is built into all modern browsers
    this.eventSource = new EventSource(url);

    this.eventSource.onmessage = (event) => {
        const parsed = JSON.parse(event.data);
        this.eventSubject.next(parsed); // Emit to subscribers
    };

    this.eventSource.onerror = () => {
        this.disconnect();
    };
}
```

**Key Points:**
- `EventSource` is a **native browser API** (no library needed)
- Automatically handles reconnection
- Parses incoming events and emits via RxJS Subject

#### 2. **Subscribing to Events** (`app.component.ts`)
```typescript
ngOnInit() {
    // Connect to SSE stream
    this.sseService.connect('http://localhost:5000/api/sse/stream');

    // React to events
    this.sseService.events$.subscribe(event => {
        if (event.eventType === 'patient_picked') {
            this.refreshQueue(); // Update UI in real-time
        }
    });
}
```

---

## Concurrency Handling (The Critical Part)

### Problem:
Two doctors (Doctor A and Doctor B) see the same patient in their queue.  
Both click "Pick Patient" at the **same time**.  
Only **one** should succeed.

### Solution: **Server-Side Locking**
```csharp
private readonly object _pickLock = new();

public Appointment? PickPatient(int appointmentId, string doctorName)
{
    lock (_pickLock)  // 🔒 Thread-safe lock
    {
        if (appointment.Status == "MappedToDoctor")
        {
            appointment.Status = "PickedByDoctor";
            return appointment;
        }
        return null; // Already picked by someone else
    }
}
```

### What Happens:
1. **Doctor A clicks "Pick"** → Request 1 arrives
2. **Doctor B clicks "Pick"** → Request 2 arrives (simultaneously)
3. Server processes **Request 1 first** (lock acquired)
4. Patient status changes to `PickedByDoctor`
5. SSE broadcasts event → **Doctor B's UI updates immediately**
6. **Request 2 arrives** (lock acquired after Request 1)
7. Server returns error → "Patient already picked"
8. **Doctor B sees error + updated UI** (patient removed from queue)

**Result:** No race condition. Only Doctor A successfully picks the patient.

---

## Pros and Cons of SSE

### ✅ **Pros:**
1. **Simple to implement** (built into browsers)
2. **No external libraries** (EventSource API is native)
3. **Automatic reconnection** (browser handles it)
4. **HTTP-based** (works through proxies/firewalls)
5. **Efficient** (single connection for unlimited events)
6. **Text-based** (easy to debug in Network tab)

### ❌ **Cons:**
1. **Unidirectional only** (server → client)
2. **Text-only** (no binary data)
3. **Browser connection limits** (6 per domain in HTTP/1.1)
4. **No built-in acknowledgment** (can't confirm client received)
5. **Not suitable for high-frequency updates** (use WebSocket for that)

---

## Best-Fit Use Cases

| Use Case                          | SSE | WebSocket | Polling |
|-----------------------------------|-----|-----------|---------|
| Real-time notifications           | ✅  | ✅        | ❌      |
| Live dashboards                   | ✅  | ✅        | ⚠️      |
| Chat applications                 | ❌  | ✅        | ❌      |
| Stock tickers                     | ✅  | ✅        | ⚠️      |
| Queue/appointment systems         | ✅  | ✅        | ❌      |
| Multiplayer games                 | ❌  | ✅        | ❌      |
| File upload progress              | ✅  | ✅        | ⚠️      |
| IoT sensor data (high-frequency)  | ❌  | ✅        | ❌      |

**Key:** ✅ = Excellent, ⚠️ = Possible but inefficient, ❌ = Not suitable

---

## Practical Tips and Common Pitfalls

### ✅ **Best Practices:**

1. **Always set correct headers:**
   ```csharp
   Response.ContentType = "text/event-stream";
   Response.Headers.Add("Cache-Control", "no-cache");
   ```

2. **End messages with double newline:**
   ```csharp
   await writer.WriteAsync($"data: {message}\n\n");
   ```

3. **Handle client disconnects gracefully:**
   ```csharp
   try {
       await writer.WriteAsync(...);
   } catch {
       // Remove from active clients
   }
   ```

4. **Use CORS for cross-origin requests:**
   ```csharp
   builder.Services.AddCors(options => {
       options.AddPolicy("AllowAngular", policy => {
           policy.WithOrigins("http://localhost:4200")
                 .AllowCredentials(); // Required for EventSource
       });
   });
   ```

5. **Implement heartbeat/keep-alive:**
   ```csharp
   // Send comment every 30 seconds to keep connection alive
   await writer.WriteAsync(": heartbeat\n\n");
   ```

### ❌ **Common Pitfalls:**

1. **Forgetting double newline** → Events won't be detected
2. **Not handling disconnects** → Memory leaks
3. **Missing CORS credentials** → Connection fails
4. **Using HTTP/2 with many connections** → Hit connection limits
5. **Not implementing reconnection logic** → Users lose updates
6. **Blocking the response stream** → Connection hangs

---

## Step-by-Step Teammate Onboarding

### **Phase 1: Understand the Concepts (30 min)**
1. Read "What is SSE?" section
2. Understand "When to Use SSE?"
3. Review the conceptual flow diagram

### **Phase 2: Explore the Code (1 hour)**
1. **Backend:**
   - Open `SseController.cs` → See how connection is established
   - Open `SseService.cs` → See how events are broadcasted
   - Open `AppointmentController.cs` → See where events are triggered

2. **Frontend:**
   - Open `sse.service.ts` → See EventSource implementation
   - Open `app.component.ts` → See how components subscribe to events

### **Phase 3: Run and Test (1 hour)**
1. Follow the setup guide in `SETUP.md`
2. Open **two browser tabs** (simulate two doctors)
3. Book an appointment → Watch SSE event in console
4. Map to doctor → Watch real-time update
5. Pick patient in Tab 1 → Watch Tab 2 update instantly

### **Phase 4: Experiment (1 hour)**
1. Add a new event type (e.g., `patient_completed`)
2. Broadcast it from backend
3. Handle it in frontend
4. Test the flow

### **Phase 5: Debugging Practice**
1. Open browser **DevTools → Network tab**
2. Find the SSE request (Type: `eventsource`)
3. See raw events flowing in
4. Intentionally break the connection → Watch reconnection

---

## Production Considerations

### **Scaling:**
1. **Sticky sessions** required (clients must reconnect to same server)
2. Use **Redis Pub/Sub** to broadcast events across multiple servers
3. Consider **nginx** or **HAProxy** for load balancing with sticky sessions

### **Monitoring:**
1. Track active SSE connections count
2. Monitor connection duration
3. Alert on high disconnection rates

### **Security:**
1. Authenticate SSE connections (pass token in query string or headers)
2. Validate user permissions before sending events
3. Rate-limit connections per IP

### **Example: Redis Pub/Sub for Multi-Server**
```csharp
// Server 1 publishes event
await _redis.PublishAsync("sse-events", message);

// All servers subscribe
_redis.Subscribe("sse-events", (channel, message) => {
    _sseService.BroadcastEvent(message);
});
```

---

## Summary

**SSE is perfect when:**
- You need real-time updates from server → client
- Updates are text-based (JSON)
- Clients don't need to push data back frequently
- You want simple implementation with native browser support

**In this project, SSE enables:**
- Real-time queue updates across all doctors
- Concurrency control (pick locks with instant feedback)
- Live appointment status for patients and staff
- Minimal complexity (no WebSocket server needed)

**Remember:** SSE is not a hammer for every nail. Use it when the unidirectional server→client flow fits your use case. For bidirectional real-time needs, consider WebSockets instead.

---

## Additional Resources

- [MDN EventSource Documentation](https://developer.mozilla.org/en-US/docs/Web/API/EventSource)
- [HTML5 SSE Specification](https://html.spec.whatwg.org/multipage/server-sent-events.html)
- [When to use SSE vs WebSockets](https://ably.com/topic/server-sent-events-vs-websockets)
