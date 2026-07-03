# 🖥️ Using Azure Storage Explorer (GUI) - Visual Guide

**Much easier than CLI!** You can see queues, messages, and everything visually.

---

## ✅ What You Already Have

I can see from your screenshot:
- ✅ Azure Storage Explorer is installed
- ✅ Connected to Local Emulator
- ✅ Can see Blob Containers, Queues, Tables

Perfect! You're ready to go!

---

## 🚀 Step-by-Step Guide

### **Step 1: Start the Emulator (One-time setup)**

Azure Storage Explorer has a **built-in emulator** - no need for Azurite CLI!

**Option A: Auto-start (Recommended)**
1. In Azure Storage Explorer, click **"Emulator & Attached"** in left panel
2. Right-click on **"Storage Accounts"**
3. Select **"Start Storage Emulator"**
4. ✅ Done! Emulator starts automatically

**Option B: Manual start**
```powershell
# If Option A doesn't work, run this once:
"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" start
```

---

### **Step 2: Verify Connection**

In Azure Storage Explorer left panel, you should see:
```
📁 Emulator & Attached
  └─ 🔑 (Emulator - Default Ports) (Key)
      ├─ 📦 Blob Containers
      ├─ 📨 Queues          ← THIS IS WHERE YOU'LL SEE MESSAGES
      └─ 📋 Tables
```

✅ **If you see this structure, you're connected!**

---

### **Step 3: Open Queues Section**

1. Click on **"Queues"** in the left panel
2. You'll see an empty list (no queues yet - that's normal!)
3. Keep this window open

---

### **Step 4: Start Your Function & Web API**

Now run your projects:

**Terminal 1: Start Azure Function**
```powershell
cd "D:\personal\Azure-leaaning\Azure-learning-hands-on\azure-notification\NotificationSystem\NotificationFunctionApp"
func start
```
✅ Wait for: `Worker process started`

**Terminal 2: Start Web API**
```powershell
cd "D:\personal\Azure-leaaning\Azure-learning-hands-on\azure-notification\NotificationSystem\NotificationWebApi"
dotnet run --urls "http://localhost:5000"
```
✅ Wait for: `Now listening on: http://localhost:5000`

**Terminal 3: Open Swagger**
```powershell
start http://localhost:5000/swagger
```

---

### **Step 5: Send a Test Notification (In Swagger)**

1. In Swagger, find: **POST /api/Notification/send-email**
2. Click **"Try it out"**
3. Enter:
```json
{
  "userId": "user-001",
  "email": "test@example.com",
  "subject": "Test Email",
  "body": "This is a test notification"
}
```
4. Click **"Execute"**

---

### **Step 6: Watch the Queue in Storage Explorer** 🎉

**Go back to Azure Storage Explorer:**

1. Click **"Refresh"** button at the top
2. Look under **Queues** in left panel
3. ✅ You should now see: **"notification-queue"** appear!
4. Click on **"notification-queue"**
5. ✅ You'll see your message!

**What you'll see:**
```
Message ID: [GUID]
Insertion Time: [Timestamp]
Expiration Time: [7 days later]
Dequeue Count: 0 (or 1 after processing)
Message Text: {"userId":"user-001","email":"test@example.com",...}
```

---

### **Step 7: Watch Messages Get Processed**

**In Storage Explorer:**
1. Keep **"notification-queue"** selected
2. Click **"Refresh"** every few seconds
3. ✅ Watch the message **disappear** (means it was processed!)

**In Function Terminal:**
```
ProcessNotificationQueue: Processing queue message
ProcessNotificationQueue: Email sent successfully to test@example.com
ProcessNotificationQueue: Notification processed successfully
```

---

## 🎯 Visual Monitoring Guide

### **To See Messages in Real-Time:**

```
┌─────────────────────────────────────────┐
│  Azure Storage Explorer                 │
├─────────────────────────────────────────┤
│  📁 Emulator & Attached                 │
│    └─ 🔑 (Emulator - Default Ports)    │
│        ├─ 📦 Blob Containers            │
│        ├─ 📨 Queues                     │
│        │   └─ notification-queue ✅     │  ← Click here!
│        └─ 📋 Tables                     │
└─────────────────────────────────────────┘

When you click "notification-queue", you see:
┌──────────────────────────────────────────────────────┐
│  Message List                                         │
├──────────────────────────────────────────────────────┤
│  📧 Message 1                                        │
│     ID: 550e8400-e29b-41d4-a716-446655440000        │
│     Insertion: 2026-06-30 10:30:00                  │
│     Dequeue Count: 0                                 │
│     [View Message Content]                           │
└──────────────────────────────────────────────────────┘
```

---

## 🎨 Cool Features in Storage Explorer

### **1. View Message Content**
- Click on any message
- Click **"View Message"** at the top
- See the full JSON payload
- Copy message for debugging

### **2. Clear Queue**
- Right-click on **"notification-queue"**
- Select **"Clear Queue"**
- Removes all messages (useful for testing)

### **3. Refresh Automatically**
- Click **"Refresh"** button periodically
- Or right-click → **"Refresh"** (F5)

### **4. Message Properties**
- **Insertion Time** - When message was added
- **Expiration Time** - When it will be deleted (default: 7 days)
- **Dequeue Count** - How many times processed (0 = not yet, 1 = processed once)
- **Pop Receipt** - Unique identifier for dequeue operation

### **5. Queue Statistics**
- Right-click queue → **"Get Statistics"**
- See total message count
- See approximate queue length

---

## 📊 Testing Workflow with GUI

### **Complete Testing Flow:**

1. **Azure Storage Explorer** - Open and connected to emulator
2. **Terminal 1** - Run Function App (`func start`)
3. **Terminal 2** - Run Web API (`dotnet run`)
4. **Browser** - Open Swagger (`http://localhost:5000/swagger`)

**Send Notification:**
1. In Swagger → Click endpoint → "Try it out" → Fill form → Execute
2. ✅ See 202 Accepted response with messageId
3. **Go to Storage Explorer** → Click Refresh
4. ✅ See message in notification-queue
5. **Wait 1-2 seconds** → Click Refresh again
6. ✅ Message disappears (processed!)
7. **Check Function Terminal** → See success logs

---

## 🐛 Troubleshooting in GUI

### **Problem: Queue doesn't appear**

**Check:**
1. Is emulator running? (Look for green checkmark in Storage Explorer)
2. Did you click Refresh?
3. Did Swagger return 202 Accepted?
4. Check Function logs for errors

**Solution:**
```powershell
# Restart emulator
"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" stop
"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" start
```

### **Problem: Messages stuck in queue (not processing)**

**Check in Storage Explorer:**
1. Click on message
2. Look at **"Dequeue Count"**
   - If 0: Function not processing (check if func start is running)
   - If 1-5: Processing failed, retrying
   - If 5+: Max retries reached, check poison queue

**Solution:**
- Check Function terminal for errors
- Verify `local.settings.json` has correct connection string
- Restart Function App

### **Problem: Too many messages in queue**

**Quick Fix:**
1. Right-click **"notification-queue"**
2. Select **"Clear Queue"**
3. All messages deleted instantly

---

## ✨ Benefits of Using GUI

✅ **Visual** - See messages with your eyes  
✅ **Easy** - Click buttons instead of typing commands  
✅ **Real-time** - Refresh to see changes  
✅ **Debugging** - View message content easily  
✅ **No CLI** - No command line knowledge needed  
✅ **Professional** - Same tool used in production Azure

---

## 🎯 Updated Startup Steps (No CLI Needed!)

### **New Simplified Steps:**

**Step 1: Start Emulator (in Storage Explorer)**
- Open Azure Storage Explorer
- Click "Start Storage Emulator" (if not running)
- ✅ See green checkmark

**Step 2: Start Function**
- Run: `START-2-AzureFunction.bat`
- ✅ Wait for "Worker process started"

**Step 3: Start Web API**
- Run: `START-3-WebAPI.bat`
- ✅ Wait for "Now listening"

**Step 4: Open Swagger**
- Run: `OPEN-SWAGGER.bat`
- ✅ Browser opens

**Step 5: Test & Monitor**
- Send notification in Swagger
- Watch queue in Storage Explorer
- See processing in Function logs

---

## 📸 Expected Screenshots

### **Before Sending Message:**
```
Queues (folder) - Empty
```

### **After Sending Message:**
```
Queues (folder)
└─ notification-queue (1 message)
    └─ Message ID: 550e8400...
```

### **After Processing:**
```
Queues (folder)
└─ notification-queue (0 messages)
```

---

## 🎓 Pro Tips

**Tip 1: Keep Storage Explorer Open**
- Don't close it during testing
- Click Refresh to see updates
- Watch messages flow through

**Tip 2: Use Message Details**
- Double-click any message
- See full JSON content
- Copy for debugging

**Tip 3: Monitor Performance**
- Check "Dequeue Count"
- 0 = Not processed yet
- 1 = Processed successfully
- 2+ = Retry attempts

**Tip 4: Clean Up After Testing**
- Right-click queue → Clear
- Remove test messages
- Start fresh

---

## ✅ Success Checklist

- [x] Azure Storage Explorer installed
- [x] Connected to local emulator
- [x] Can see Queues section
- [ ] Emulator running (green checkmark)
- [ ] Function App running
- [ ] Web API running
- [ ] Sent test notification via Swagger
- [ ] Saw message in notification-queue
- [ ] Message processed and disappeared
- [ ] Function logs show success

---

**Ready to test? No CLI commands needed - just use the GUI!** 🎉
