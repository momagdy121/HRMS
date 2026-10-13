# 🏢 HRMS — Human Resource Management System

A full-featured, role-based Human Resource Management System built with **ASP.NET Core 10 MVC**, **Entity Framework Core**, **ASP.NET Core Identity**, and **SQL Server**. The system covers the complete employee lifecycle — from recruitment and onboarding through daily HR operations and payroll.

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Roles & Permissions](#-roles--permissions)
- [Module Overview](#-module-overview)
- [Database Schema](#-database-schema)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Running Migrations](#-running-migrations)
- [Email & Cloudinary Setup](#-email--cloudinary-setup)
- [Default Seeded Accounts](#-default-seeded-accounts)
- [Project Structure](#-project-structure)
- [Security Notes](#-security-notes)

---

## ✨ Features

| Module | Highlights |
|---|---|
| **Authentication** | ASP.NET Core Identity, forced password change on first login, account lockout |
| **Recruitment** | Public job board, CV upload, application review, CEO-only onboarding |
| **Employees** | Full CRUD, profile pictures via Cloudinary, department assignment |
| **Departments** | Hierarchy, department head assignment, employee counts |
| **Attendance** | Daily clock-in/out, monthly reports |
| **Leave Management** | Request, approve/reject, leave balance tracking |
| **Payroll** | Generate, approve, and export payslips with itemised deductions/allowances |
| **Task Management** | Assign tasks to employees, email notifications, completion tracking |
| **Notifications** | In-app notification centre per user |
| **Email** | Transactional emails via Brevo (Sendinblue) API |
| **File Storage** | Cloudinary for profile pictures and CV documents |

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core 10 MVC |
| **ORM** | Entity Framework Core 10 |
| **Database** | SQL Server |
| **Identity** | ASP.NET Core Identity (merged with `Employee` entity) |
| **Email** | Brevo (Sendinblue) — `sib_api_v3_sdk` |
| **File Storage** | Cloudinary — `CloudinaryDotNet` |
| **UI** | Razor Views, Bootstrap, custom CSS |
| **Auth** | Cookie-based authentication, role-based authorization |

---

## 🏗 Architecture

The solution follows a clean **N-Tier / layered architecture** with four projects:

```
HRSystem.sln
├── HRSystem.Common        # Shared enums, constants, role definitions
├── HRSystem.Data          # EF Core entities, DbContext, repositories, UoW, migrations
├── HRSystem.Business      # Services, DTOs, interfaces, Cloudinary, Email integration
└── HRSystem.Web           # ASP.NET Core MVC — controllers, views, view-models, filters
```

### Key Patterns

- **Repository + Unit of Work** — `IUnitOfWork` wraps all repositories; services never touch `DbContext` directly
- **Service Layer** — All business logic lives in `HRSystem.Business.Services`; controllers only handle HTTP concerns
- **EF Core Fluent Configuration** — Each entity has its own `IEntityTypeConfiguration<T>` class
- **Global Filters** — `HrmsExceptionFilter` (error handling) and `RequirePasswordChangeFilter` (first-login enforcement) registered globally
- **Areas** — The `HR` area groups all authenticated management pages; public pages live in the root MVC area

---

## 👥 Roles & Permissions

| Role | Description |
|---|---|
| **CEO** | Full system access — manages all modules, recruitment, and employee accounts |
| **DepartmentHead** | Manages own department employees, approves leave, assigns tasks |
| **Employee** | Self-service — view own data, submit leave requests, view payslips |
| *(Public)* | Browse published job postings and submit applications (no account required) |

> `HR` is a backward-compatibility alias for `CEO` — they resolve to the same role.

---

## 📦 Module Overview

### 🔐 Authentication
- Login / Logout with cookie authentication
- **Forced password change** on first login (`IsPasswordChangeRequired` flag)
- Account lockout after 5 failed attempts (5-minute lockout window)
- Password requirements: 8+ chars, digit, upper & lower case

### 📣 Recruitment *(CEO only)*
- **Job Postings** — Draft → Published → Closed lifecycle; only published postings appear publicly
- **Public Careers Page** — No login required; lists open positions with details and apply action
- **Application Form** — Candidates submit name, email, phone, cover letter, and CV (PDF/DOC/DOCX, max 5 MB)
- **CV Storage** — Uploaded securely to Cloudinary as a private raw asset; never publicly accessible
- **Application Management** — CEO can filter by posting/status, read notes, preview/download CVs, and accept or reject candidates
- **State Machine** — `Submitted → Accepted | Rejected` (enforced server-side)
- **Rejection Email** — Sent automatically on rejection; failures surfaced to CEO without corrupting the record
- **Onboarding** — CEO explicitly creates an Employee account from an accepted application; reuses existing Identity creation flow with `IsPasswordChangeRequired = true`
- **Acceptance Email** — Sent only after successful account creation; retry-safe (no duplicate accounts)

### 👤 Employees
- Create, edit, soft-delete employees
- Upload profile pictures (Cloudinary)
- Assign to departments and roles
- Role hierarchy enforced

### 🏢 Departments
- Create and manage departments
- Assign a Department Head
- View department members and details

### 📅 Attendance
- Clock in / Clock out
- Monthly attendance reports per employee

### 🌴 Leave Management
- Employee submits leave requests (Annual, Sick, Unpaid, etc.)
- Department Head / CEO approves or rejects
- Leave balance tracking per employee per year

### 💰 Payroll
- Generate monthly payroll per employee
- Add allowances and deductions (itemised `PayrollItem`)
- Approve and finalize payslips

### ✅ Tasks
- Assign tasks to employees with due dates
- Email notification sent to the employee on assignment
- Employee marks task as complete with notes

### 🔔 Notifications
- In-app notification centre per user
- Triggered on key workflow events (leave approval, task assignment, etc.)

---

## 🗄 Database Schema

### Core Entities

| Entity | Key Fields |
|---|---|
| `Employee` | `Id`, `FirstName`, `LastName`, `DepartmentId`, `Salary`, `HireDate`, `IsActive`, `IsPasswordChangeRequired`, `ProfilePictureUrl` |
| `Department` | `Id`, `Name`, `ManagerId` |
| `Attendance` | `Id`, `EmployeeId`, `Date`, `ClockIn`, `ClockOut` |
| `LeaveRequest` | `Id`, `EmployeeId`, `LeaveType`, `Status`, `StartDate`, `EndDate` |
| `LeaveBalance` | `Id`, `EmployeeId`, `Year`, `TotalDays`, `UsedDays` |
| `Payroll` | `Id`, `EmployeeId`, `Month`, `Year`, `Status`, `NetSalary` |
| `PayrollItem` | `Id`, `PayrollId`, `ItemType`, `Description`, `Amount` |
| `EmployeeTask` | `Id`, `EmployeeId`, `AssignedById`, `Title`, `DueDate`, `Status` |
| `UserNotification` | `Id`, `UserId`, `Message`, `IsRead`, `CreatedAt` |

### Recruitment Entities

| Entity | Key Fields |
|---|---|
| `JobPosting` | `Id`, `Title`, `Description`, `Requirements`, `DepartmentId`, `ClosingDate`, `Status`, `CreatedById`, `CreatedAt`, `PublishedAt`, `ClosedAt` |
| `JobApplication` | `Id`, `JobPostingId`, `FullName`, `Email`, `PhoneNumber`, `CvPublicId`, `CvOriginalFileName`, `CvContentType`, `CvFileSize`, `CoverLetter`, `Status`, `SubmittedAt`, `ReviewNotes`, `ReviewedById`, `ReviewedAt`, `CreatedEmployeeId`, `RejectionEmailSent`, `AcceptanceEmailSent` |

### Enums

| Enum | Values |
|---|---|
| `JobPostingStatus` | `Draft`, `Published`, `Closed` |
| `JobApplicationStatus` | `Submitted`, `Accepted`, `Rejected` |
| `LeaveRequestStatus` | `Pending`, `Approved`, `Rejected` |
| `PayrollStatus` | `Draft`, `Approved` |
| `TaskStatus` | `Pending`, `InProgress`, `Completed` |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or remote)
- A [Cloudinary](https://cloudinary.com) account (free tier works)
- A [Brevo](https://www.brevo.com) account for transactional email (free tier works)

### 1. Clone the Repository

```bash
git clone https://github.com/<your-username>/HRMS.git
cd HRMS
```

### 2. Configure the Application

Fill in your credentials in `appsettings.Development.json` or use User Secrets (see [Configuration](#-configuration)).

### 3. Apply Migrations

```bash
dotnet ef database update --project HRSystem.Data --startup-project HRSystem.Web
```

> The app will also auto-seed roles and the default CEO account on first run.

### 4. Run the Application

```bash
dotnet run --project HRSystem.Web
```

Navigate to `https://localhost:5001` (or the port shown in your terminal).

---

## ⚙️ Configuration

Edit `HRSystem.Web/appsettings.json` — or, for local development, use `appsettings.Development.json` / [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.;Database=HRMS;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
  },
  "BravoApi": {
    "ApiKey": "YOUR_BREVO_API_KEY",
    "SenderEmail": "no-reply@yourdomain.com",
    "SenderName": "HRMS Portal"
  },
  "CloudinarySettings": {
    "CloudName": "YOUR_CLOUD_NAME",
    "ApiKey": "YOUR_CLOUDINARY_API_KEY",
    "ApiSecret": "YOUR_CLOUDINARY_API_SECRET"
  }
}
```

> ⚠️ **Never commit real API keys to source control.**

### Recommended: User Secrets (Development)

```bash
cd HRSystem.Web
dotnet user-secrets init
dotnet user-secrets set "CloudinarySettings:CloudName" "your_cloud_name"
dotnet user-secrets set "CloudinarySettings:ApiKey" "your_api_key"
dotnet user-secrets set "CloudinarySettings:ApiSecret" "your_api_secret"
dotnet user-secrets set "BravoApi:ApiKey" "your_brevo_key"
```

---

## 🗃 Running Migrations

```bash
# Apply all pending migrations
dotnet ef database update --project HRSystem.Data --startup-project HRSystem.Web

# Add a new migration
dotnet ef migrations add <MigrationName> --project HRSystem.Data --startup-project HRSystem.Web

# Generate a SQL script (safe for production review)
dotnet ef migrations script --project HRSystem.Data --startup-project HRSystem.Web --output migration.sql
```

---

## 📧 Email & Cloudinary Setup

### Brevo (Email)

1. Sign up at [brevo.com](https://www.brevo.com)
2. Go to **SMTP & API → API Keys** and create a new key
3. Set `BravoApi:ApiKey` and verify your sender email address in Brevo

### Cloudinary (File Storage)

1. Sign up at [cloudinary.com](https://cloudinary.com)
2. Copy your **Cloud Name**, **API Key**, and **API Secret** from the Dashboard
3. Set `CloudinarySettings:CloudName`, `CloudinarySettings:ApiKey`, `CloudinarySettings:ApiSecret`

> CV files are uploaded as **private raw assets** and served only through an authorized backend proxy — they are never publicly accessible.

---

## 🔑 Default Seeded Accounts

On first run the application seeds the following accounts:

| Role | Email | Temp Password |
|---|---|---|
| CEO | `ceo@hrms.com` | `Admin@123` |
| Department Head | *(seeded per department)* | `Admin@123` |
| Employee | *(sample employees)* | `Admin@123` |

> All seeded accounts require a password change on first login.

---

## 📁 Project Structure

```
HRMS/
├── HRSystem.Common/
│   ├── Constants/
│   │   ├── RoleNames.cs              # CEO, DepartmentHead, Employee
│   │   └── RoleHierarchy.cs
│   └── Enums/                        # All domain enums
│
├── HRSystem.Data/
│   ├── Models/                       # EF Core entities
│   ├── Configurations/               # Fluent API entity configs
│   ├── Context/AppDbContext.cs       # Main DbContext
│   ├── Interfaces/                   # IUnitOfWork + repository interfaces
│   ├── Repositories/                 # Generic + specific repositories, UnitOfWork
│   └── Migrations/                   # EF Core migration history
│
├── HRSystem.Business/
│   ├── Interfaces/Services/          # Service contracts
│   ├── Services/                     # Business logic implementations
│   │   ├── RecruitmentService.cs     # Full recruitment workflow
│   │   ├── CloudinaryService.cs      # Images + private CV documents
│   │   ├── BrevoEmailService.cs      # Transactional emails
│   │   └── EmployeeService.cs        # Employee + Identity account creation
│   ├── DTOs/                         # Data transfer objects
│   ├── Helpers/
│   │   ├── EmailTemplates.cs         # HTML email templates
│   │   └── CvFileValidator.cs        # Signature-based file validation
│   └── Mapping/                      # Mapping profiles
│
└── HRSystem.Web/
    ├── Areas/HR/
    │   ├── Controllers/              # CEO & DepartmentHead controllers
    │   └── Views/                    # Razor views for management UI
    ├── Controllers/                  # Public + account controllers
    │   ├── CareersController.cs      # Public job board & application form
    │   └── AccountController.cs      # Login, logout, password change
    ├── Views/
    │   ├── Careers/                  # Public job listings and application form
    │   └── Shared/                   # Layouts, sidebars, partials
    ├── ViewModels/                   # Strongly-typed view models per feature
    ├── Filters/
    │   ├── HrmsExceptionFilter.cs    # Global exception handling
    │   └── RequirePasswordChangeFilter.cs  # First-login enforcement
    ├── DataSeeding/                  # Identity roles & user seeding
    └── Program.cs                    # App startup, DI, middleware pipeline
```

---

## 🔒 Security Notes

- **Role enforcement** is applied server-side via `[Authorize(Roles = ...)]` — UI hiding is never the sole security mechanism.
- **Anti-forgery tokens** are validated on all state-changing form submissions.
- **CV files** are Cloudinary private raw assets, accessible only through a CEO-authorized backend proxy endpoint.
- **File uploads** are validated server-side using **magic byte (file signature)** inspection — extension and MIME type alone are not trusted.
- **Status transitions** (`Submitted → Accepted/Rejected`) are enforced in the service layer; payload manipulation cannot bypass the state machine.
- **Duplicate account prevention** — Employee creation from an accepted application is idempotent; re-submitting is detected and blocked.
- **Passwords** are never logged or stored in plaintext. First-login forced password change is always preserved.
- **API secrets** must be stored in environment variables or User Secrets — `appsettings.json` contains only placeholders.

---

## 📄 License

This project is for educational and portfolio purposes.
