# Setup Guide - SSE Appointment Queue System

## Prerequisites

Before starting, ensure you have the following installed:

- **Node.js** (v18 or higher) - [Download](https://nodejs.org/)
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Angular CLI** - Install via: `npm install -g @angular/cli`
- **Code Editor** - VS Code recommended

Verify installations:
```bash
node --version    # Should show v18.x or higher
dotnet --version  # Should show 8.0.x or higher
ng version        # Should show Angular CLI version
```

---

## Project Structure

```
sse-handson/
├── backend/
│   └── AppointmentQueueAPI/
│       ├── Controllers/
│       │   ├── AppointmentController.cs
│       │   └── SseController.cs
│       ├── Models/
│       │   └── Appointment.cs
│       ├── Services/
│       │   ├── AppointmentService.cs
│       │   └── SseService.cs
│       ├── Program.cs
│       └── AppointmentQueueAPI.csproj
├── frontend/
│   └── appointment-queue-ui/
│       ├── src/
│       │   ├── app/
│       │   │   ├── services/
│       │   │   │   ├── sse.service.ts
│       │   │   │   └── appointment.service.ts
│       │   │   └── app.component.ts
│       │   ├── main.ts
│       │   ├── index.html
│       │   └── styles.css
│       ├── package.json
│       └── angular.json
├── SSE-DOCUMENTATION.md
└── SETUP.md (this file)
```

---

## Backend Setup (.NET Web API)

### Step 1: Navigate to Backend Directory
```bash
cd backend/AppointmentQueueAPI
```

### Step 2: Restore Dependencies
```bash
dotnet restore
```

### Step 3: Run the Backend
```bash
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Step 4: Verify Backend is Running

Open your browser and navigate to:
- **Swagger UI:** http://localhost:5000/swagger
- **SSE Status:** http://localhost:5000/api/sse/status

You should see the Swagger documentation with available endpoints.

---

## Frontend Setup (Angular)

### Step 1: Navigate to Frontend Directory
Open a **new terminal** (keep backend running) and navigate:
```bash
cd frontend/appointment-queue-ui
```

### Step 2: Install Dependencies
```bash
npm install
```

**Note:** This may take a few minutes on first run.

### Step 3: Run the Frontend
```bash
npm start
```

**Expected Output:**
```
** Angular Live Development Server is listening on localhost:4200 **
✔ Compiled successfully.
```

### Step 4: Open the Application

Open your browser and navigate to:
- **http://localhost:4200**

You should see the **SSE Appointment Queue Demo** interface.

---

## Verification Steps

### 1. Check SSE Connection

- Open the application at http://localhost:4200
- Open **Browser DevTools** (F12)
- Go to **Console** tab
- You should see:
  ```
  🔌 Connecting to SSE endpoint: http://localhost:5000/api/sse/stream
  ✅ SSE connection established
  ```

- Go to **Network** tab
- Find the request to `/api/sse/stream`
- **Type:** should show `eventsource`
- **Status:** should show `200` and remain open

### 2. Test Patient Booking

1. Ensure **"Patient"** role is selected
2. Enter a patient name (e.g., "John Doe")
3. Click **"Book Appointment"**
4. Check the console for:
   ```
   📩 SSE Event received: {eventType: "appointment_booked", ...}
   ```
5. Appointment should appear in the list

### 3. Test Staff Mapping

1. Switch role to **"Staff"**
2. Select the appointment you just created
3. Enter a doctor name (e.g., "DrSmith")
4. Click **"Map to Doctor"**
5. Watch the SSE event log - you should see:
   ```
   patient_mapped - [timestamp]
   ```

### 4. Test Doctor Queue (Single Browser)

1. Switch role to **"Doctor"**
2. Enter the doctor name you used (e.g., "DrSmith")
3. Click **"Refresh My Queue"**
4. You should see the mapped patient
5. Click **"Pick Patient"**
6. Watch SSE event:
   ```
   patient_picked - [timestamp]
   ```
7. Patient moves to "My Picked Patients" section

---

## Testing Concurrency (Multiple Browsers)

This is the **key demonstration** of SSE's real-time capabilities.

### Step 1: Open Two Browser Windows

- **Window 1:** http://localhost:4200 (Chrome)
- **Window 2:** http://localhost:4200 (Incognito/Private Window)

### Step 2: Setup Both as Doctors

**In Window 1:**
- Switch to "Doctor" role
- Enter doctor name: "DrSmith"
- Click "Refresh My Queue"

**In Window 2:**
- Switch to "Doctor" role
- Enter doctor name: "DrJones"
- Click "Refresh My Queue"

### Step 3: Create a Patient and Map to Both Doctors

**In a third window (as Staff):**
1. Switch to "Patient" role
2. Book appointment for "Patient A"
3. Switch to "Staff" role
4. Map "Patient A" to "DrSmith"

### Step 4: Watch Real-Time Updates

- **Window 1 (DrSmith)** should immediately show "Patient A" in the queue
- The SSE event log should show `patient_mapped` event

### Step 5: Test Concurrency Lock

**In Window 1 (DrSmith):**
- Click "Pick Patient" for "Patient A"

**Watch Window 2 (DrJones) - This is the magic!**
- ❌ If Dr Smith picks the patient, Dr Jones should see it removed from their queue **instantly**
- No refresh needed - SSE broadcasts the update

**Try picking from Window 2:**
- If you try to pick after Dr Smith, you'll get an error
- Backend prevents concurrent picking with server-side lock

---

## Troubleshooting

### Backend Issues

**Problem:** Backend won't start / Port already in use
```
Failed to bind to address http://localhost:5000
```

**Solution:**
```bash
# Find process using port 5000
netstat -ano | findstr :5000    # Windows
lsof -i :5000                   # Mac/Linux

# Kill the process or change port in Program.cs
```

**Problem:** CORS errors in browser console
```
Access to fetch at 'http://localhost:5000' blocked by CORS policy
```

**Solution:**
- Verify CORS is configured in `Program.cs`
- Ensure `AllowCredentials()` is set (required for EventSource)
- Frontend must run on http://localhost:4200 exactly

---

### Frontend Issues

**Problem:** `npm install` fails
```
npm ERR! code ERESOLVE
```

**Solution:**
```bash
npm install --legacy-peer-deps
```

**Problem:** SSE connection shows disconnected
```
🔴 SSE Disconnected
```

**Solution:**
- Ensure backend is running (check http://localhost:5000/api/sse/status)
- Check browser DevTools → Network tab for failed requests
- Verify CORS headers in response

**Problem:** Events not appearing in UI
```
Connected but no events showing
```

**Solution:**
- Open DevTools → Console
- Check for JavaScript errors
- Verify SSE messages in Network tab (Type: eventsource)
- Ensure event format matches: `data: {json}\n\n`

---

### SSE-Specific Debugging

**How to see raw SSE events:**

1. Open **DevTools → Network tab**
2. Filter by **Type: EventSource**
3. Click on `/api/sse/stream` request
4. Go to **"EventStream"** tab (Chrome) or **"Response"** tab (Firefox)
5. You'll see raw events as they arrive:
   ```
   data: {"eventType":"connected","message":"SSE connection established"}

   data: {"eventType":"appointment_booked","data":{...}}

   ```

**Expected SSE Headers:**
```
Content-Type: text/event-stream
Cache-Control: no-cache
Connection: keep-alive
Access-Control-Allow-Origin: http://localhost:4200
Access-Control-Allow-Credentials: true
```

---

## Running Multiple Clients for Testing

### Option 1: Multiple Browser Tabs
- Open 3 tabs in the same browser
- Each maintains its own SSE connection
- Use different roles in each tab

### Option 2: Multiple Browsers
- Chrome, Firefox, Edge, etc.
- Each browser independently connects
- Better simulation of real users

### Option 3: Private/Incognito Windows
- Separate session per incognito window
- Useful for testing different users

---

## Development Tips

### Hot Reload

**Backend:**
```bash
dotnet watch run
```
- Auto-reloads on file changes
- No need to restart manually

**Frontend:**
- Already enabled by default with `npm start`
- Changes reflect instantly in browser

### Useful Commands

**Backend:**
```bash
# Build only (no run)
dotnet build

# Clean build artifacts
dotnet clean

# Run tests (if you add any)
dotnet test
```

**Frontend:**
```bash
# Production build
npm run build

# Lint TypeScript
ng lint

# Type check
npx tsc --noEmit
```

---

## Next Steps

Once you have the application running:

1. **Read the SSE Documentation** (`SSE-DOCUMENTATION.md`)
   - Understand the concepts
   - Learn how SSE works under the hood
   - See the code explanations

2. **Experiment with the Code**
   - Add a new event type (e.g., `appointment_completed`)
   - Try broadcasting custom data
   - Implement a heartbeat/keep-alive mechanism

3. **Test Edge Cases**
   - What happens if backend crashes?
   - How does reconnection work?
   - Can you handle 10+ concurrent connections?

4. **Extend the Features**
   - Add authentication (JWT tokens)
   - Implement PostgreSQL database
   - Add appointment cancellation
   - Create a chat feature for doctor-patient communication

---

## Architecture Overview

```
┌─────────────────┐
│  Browser 1      │
│  (Angular)      │──┐
└─────────────────┘  │
                     │  SSE Connection
┌─────────────────┐  │  (EventSource)
│  Browser 2      │──┤
│  (Angular)      │  │
└─────────────────┘  │
                     │
┌─────────────────┐  │
│  Browser 3      │──┘
│  (Angular)      │
└─────────────────┘
         │
         │ HTTP POST (Book, Map, Pick)
         ▼
┌─────────────────────────────────┐
│   .NET Web API                  │
│                                 │
│  ┌─────────────────────────┐   │
│  │  AppointmentController   │   │
│  │  - Book()                │   │
│  │  - Map()                 │   │
│  │  - Pick() ──────────┐    │   │
│  └──────────────────────┘    │   │
│                          │    │   │
│  ┌────────────────────── ▼ ──┐   │
│  │  SseService               │   │
│  │  - BroadcastEvent()       │   │
│  │    (Sends to ALL clients) │   │
│  └───────────────────────────┘   │
│                                 │
└─────────────────────────────────┘
```

---

## Common Questions

**Q: Do I need a database?**
A: No, this demo uses in-memory storage for simplicity. Data is lost on restart.

**Q: Can I use HTTPS?**
A: Yes, but you'll need SSL certificates for both frontend and backend.

**Q: How many clients can connect?**
A: This demo can handle 100+ concurrent connections easily. For production scale, consider Redis Pub/Sub.

**Q: What happens if a client disconnects?**
A: Browser automatically reconnects. Server detects disconnect and cleans up resources.

**Q: Can I send data from client to server via SSE?**
A: No, SSE is unidirectional (server → client). Use HTTP POST/PUT for client → server.

---

## Success Checklist

✅ Backend running on http://localhost:5000  
✅ Frontend running on http://localhost:4200  
✅ SSE connection shows "🟢 Connected"  
✅ Can book appointments as Patient  
✅ Can map to doctor as Staff  
✅ Can pick patients as Doctor  
✅ SSE events appear in real-time log  
✅ Two browsers show concurrent updates  
✅ Concurrency lock prevents double-picking  

If all checkboxes pass, you're ready to dive deep! 🎉

---

## Support

If you encounter issues not covered here:
1. Check the **SSE-DOCUMENTATION.md** for conceptual help
2. Review browser DevTools console and network tab
3. Verify all prerequisites are correctly installed
4. Ensure both backend and frontend are running simultaneously

Happy learning! 🚀
