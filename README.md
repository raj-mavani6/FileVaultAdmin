<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/MongoDB-7.0-47A248?style=for-the-badge&logo=mongodb&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-13-239120?style=for-the-badge&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" />
  <img src="https://img.shields.io/badge/Panel-Admin-E74C3C?style=for-the-badge" />
</p>

<h1 align="center">FileVault Admin — Control Panel</h1>

<p align="center">
  <strong>A dedicated administrative dashboard for the FileVault cloud storage platform.</strong><br/>
  Full platform oversight · User management · Analytics · File inspection · Share link control
</p>

---

## Features

### Dashboard & Analytics
- **Real-time overview** — total users, files, storage consumed, recent activity feed
- **Storage breakdown** by file type (images, videos, documents, archives, etc.)
- **Upload trends** — daily/weekly/monthly file upload statistics
- **Top users** ranked by storage consumption
- **File type distribution** with visual charts

### User Management
- View all registered users with email, role, status, and storage usage
- **Toggle active/inactive** status to enable or disable user accounts
- **Role assignment** — promote users to Admin or demote to User
- Per-user storage consumption tracking

### File Browser & Inspector
- Browse **all files** across all users with search, filter by owner, and folder navigation
- **Universal File Inspector** — admin-grade preview supporting 30+ file types:
  - Images, video, audio, PDF (native)
  - DOCX (local rendering via **Mammoth.js**)
  - PPTX/XLSX (thumbnail extraction via **JSZip**)
  - Office files (cloud rendering via **Microsoft Office Online Viewer**)
  - Archives (metadata extraction via **SharpCompress**)
  - Source code & text files with syntax display
  - Large file protection (1MB streaming limit)
- Upload files **on behalf of any user**
- Rename and permanently delete files

### Folder Management
- View, create, rename, and delete folders across all users
- Hierarchical folder tree browsing

### Share Link Oversight
- View **all active share links** across the platform
- Monitor access counts and last-accessed timestamps
- **Revoke** any share link instantly

### Trash Management
- View trashed files and folders across all users
- **Restore** or **permanently purge** any item
- Bulk trash operations

### Audit Logging
- Complete activity trail with:
  - User ID & email
  - Action type (upload, delete, share, login, etc.)
  - Target type & ID
  - Custom metadata
  - IP address
  - Timestamp

### Premium UI/UX
- **"Sienna & Cream"** dark-first design system matching the main platform
- Glassmorphism sidebar with backdrop blur
- Smooth scroll-triggered animations
- Dark/Light theme toggle (persisted in `localStorage`)
- Custom toast notification system
- Google Fonts (Inter) typography
- Fully responsive layout

---

## Architecture

```
FileVaultAdmin/
│
├── Program.cs                      # Entry point, DI, auth config
│
├── Controllers/
│   ├── AuthController.cs           # Admin login/logout
│   ├── HomeController.cs           # Dashboard & analytics overview
│   ├── UsersController.cs          # User CRUD, roles, status toggle
│   ├── FilesController.cs          # File browser, inspector, upload
│   ├── FoldersController.cs        # Folder management
│   ├── SharesController.cs         # Share link oversight & revocation
│   ├── TrashController.cs          # Trash management (restore/purge)
│   └── AnalyticsController.cs      # Advanced analytics & reporting
│
├── Services/
│   └── AdminService.cs             # Centralized admin operations (28KB)
│                                    # Handles all DB queries for the admin panel
│
├── Data/
│   └── MongoDbContext.cs            # Shared MongoDB connection
│                                    # Connects to same DB as main FileVault app
│
├── Models/
│   ├── Domain/                      # Shared domain entities
│   ├── ViewModels/                  # Admin-specific view models
│   └── Settings/                    # Configuration POCOs
│
├── Helpers/
│   └── FormatHelpers.cs             # File size formatting, utilities
│
├── Views/
│   ├── Auth/                        # Login page
│   ├── Home/                        # Dashboard
│   ├── Users/                       # User management views
│   ├── Files/                       # File browser & inspector
│   ├── Folders/                     # Folder browser
│   ├── Shares/                      # Share link manager
│   ├── Trash/                       # Trash manager
│   ├── Analytics/                   # Analytics views
│   └── Shared/                      # Layout, partials
│
└── wwwroot/
    ├── css/admin.css                # Admin design system (930+ lines)
    └── js/admin.js                  # Admin client-side scripts
```

---

## Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Backend | ASP.NET Core | .NET 9.0 |
| Language | C# | 13 |
| Database | MongoDB | 7.x |
| File Storage | MongoDB GridFS | 2.30.0 |
| Password Hashing | BCrypt.Net-Next | 4.0.3 |
| Logging | Serilog (Console + File) | 9.0.0 |
| Archive Handling | SharpCompress | 0.38.0 |
| Frontend | Bootstrap 5 + Vanilla JS | 5.3 |
| Icons | Bootstrap Icons | Latest |
| Typography | Google Fonts (Inter) | — |
| DOCX Preview | Mammoth.js (CDN) | 1.6.0 |
| PPTX Thumbnails | JSZip (CDN) | 3.10.1 |
| Office Preview | Microsoft Office Online Viewer | — |

---

## Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MongoDB 7+](https://www.mongodb.com/try/download/community) running on `localhost:27017`
- The **FileVault** main application must have been run at least once to seed the database

### 1. Run the Admin Panel

```bash
cd FileVaultAdmin
dotnet restore
dotnet run
```

The admin panel starts at **https://localhost:5003**

### 2. Login

Use an account with the **Admin** role from the main FileVault database:

| Field | Value |
|---|---|
| Email | `admin@filevault.local` |
| Password | `Admin@123` |

> **Note:** The Admin Panel connects to the **same MongoDB database** (`FileVault`) as the main app. Both applications share users, files, folders, and all other collections.

---

## Shared Database

The Admin Panel does **not** have its own database. It connects to the same MongoDB instance and database as the main FileVault application:

```
MongoDB: mongodb://localhost:27017
Database: FileVault
```

### Collections Accessed

| Collection | Admin Operations |
|---|---|
| `users` | View, toggle status, assign roles |
| `files` | Browse, inspect, upload, rename, delete |
| `folders` | Browse, create, rename, delete |
| `shareLinks` | View, revoke |
| `auditLogs` | Read activity trail |
| `uploadSessions` | Monitor upload states |
| `fileVaultFiles.files` | GridFS file metadata (stream for preview) |
| `fileVaultFiles.chunks` | GridFS binary data (stream for preview) |

---

## Security

| Feature | Implementation |
|---|---|
| Authentication | Cookie-based, **8-hour** sliding expiration |
| Cookie Config | `HttpOnly`, `Secure`, `SameSite=Lax` |
| Admin-Only Access | `[Authorize]` attribute on all controllers |
| Password Storage | BCrypt hashing (shared with main app) |
| Session Isolation | Separate cookie name (`FileVaultAdmin.Auth`) |
| Structured Logging | Serilog with daily rolling file logs |

---

## Admin Pages

| Page | Route | Description |
|---|---|---|
| Dashboard | `/` | Overview stats, storage charts, activity feed |
| Users | `/Users` | User list with search, status toggle, role management |
| Files | `/Files` | File browser with owner filter, search, folder navigation |
| File Inspector | `/Files/ViewFile/{id}` | Full file preview with metadata and admin actions |
| Folders | `/Folders` | Folder tree browser |
| Shares | `/Shares` | All share links with access stats and revocation |
| Trash | `/Trash` | Trashed items across all users |
| Analytics | `/Analytics` | Advanced charts, storage breakdown, upload trends |

---

## Related

- **[FileVault](../FileVault)** — Main user-facing cloud file storage application

---

## License

MIT — See [LICENSE](LICENSE) for details.
