# Azure Notification System

**Complete notification system using Azure Functions + Web API**

---

## 📖 Documentation

**👉 Main Guide:** [MANUAL.md](MANUAL.md) - Complete documentation  
**👉 GUI Guide:** [STORAGE-EXPLORER-GUIDE.md](STORAGE-EXPLORER-GUIDE.md) - Visual testing with Azure Storage Explorer

---

## 🚀 Quick Start (30 seconds - GUI Method)

1. **Open:** Azure Storage Explorer (you already have it!)
2. **Verify:** Emulator is running (green checkmark)
3. **Double-click:** `START-2-AzureFunction.bat` (wait for "Worker process started")
4. **Double-click:** `START-3-WebAPI.bat` (wait for "Now listening")
5. **Double-click:** `OPEN-SWAGGER.bat` (browser opens)
6. **Test:** Click "Try it out" → Fill form → Execute
7. **Watch:** In Storage Explorer, click "Queues" → See "notification-queue" → Watch message disappear!

---

## 📂 Project Structure

```
azure-notification/
├── NotificationSystem/          ← Main solution (open in VS)
│   ├── NotificationFunctionApp/ → Azure Function
│   └── NotificationWebApi/      → Web API with Swagger
│
├── MANUAL.md                    ← Complete documentation
├── STORAGE-EXPLORER-GUIDE.md    ← GUI testing guide
│
├── START-1-StorageExplorer.bat → Instructions for GUI
├── START-2-AzureFunction.bat   → Start function
├── START-3-WebAPI.bat          → Start API
└── OPEN-SWAGGER.bat            → Open browser
```

---

## ✅ What This Does

Sends notifications via:
- ✉️ Email
- 📱 SMS  
- 🔔 Push

**Visual Monitoring:** Watch messages flow through queues in Azure Storage Explorer GUI!

---

## 📚 Documentation Files

| File | Purpose |
|------|---------|
| [MANUAL.md](MANUAL.md) | Complete guide (architecture, code, deployment) |
| [STORAGE-EXPLORER-GUIDE.md](STORAGE-EXPLORER-GUIDE.md) | Visual GUI testing guide |

---

**Ready?** Start with Azure Storage Explorer GUI! 🚀
