# Doc2Pod — PDF to Egyptian Arabic Podcast Generator

**Doc2Pod** is a backend API that transforms PDF documents into AI-generated **Egyptian Arabic podcasts**. Upload a PDF, pick a generation mode, and receive an audio file in Egyptian Arabic dialect.

> ⚠️ This repository contains the **.NET backend only**. The AI processing (RAG, TTS, dialect adaptation) is handled by a separate Python microservice communicated with over HTTP.

---

## 🏗️ Architecture

Doc2Pod follows **Clean Architecture** with a strict separation of concerns across four layers:

```
Doc2Pod.sln
├── API/                        # ASP.NET Core Web API (entry point, controllers, middleware)
├── Application/                # Use cases, commands/queries (CQRS via MediatR), interfaces
├── Domain/                     # Entities, value objects, domain events, business rules
└── Infrastructure/             # EF Core, SQL Server, Identity, JWT, email, file storage, Python service proxy
```

### Layer Responsibilities

| Layer | Role |
|---|---|
| **API** | HTTP controllers, Swagger, middleware pipeline, DI wiring |
| **Application** | CQRS handlers (MediatR), FluentValidation, AutoMapper, service interfaces |
| **Domain** | `User`, `Document`, `Podcast` aggregates; value objects; domain events; shared kernel (`Result<T>`, `Error`, `ValueObject`) |
| **Infrastructure** | EF Core + SQL Server, ASP.NET Identity, JWT tokens, TOTP, email (MailKit/SMTP), local file storage, HTTP proxy to Python RAG service |

---

## ✨ Features

### 🎙️ Podcast Generation
- Upload any PDF document via REST API
- Three generation modes:
  - `Query` (1) — Generate podcast around a specific topic/question
  - `PageRange` (2) — Generate from a specific page range
  - `Full` (3) — Generate from the entire document
- Delegates AI processing to a Python microservice (RAG + TTS pipeline)
- Async task-based polling: get a `taskId` back, poll for status until audio is ready
- Returns an `AudioPath` when generation is complete

### 🔐 Authentication & Security
- Register / Login / Logout
- JWT Bearer tokens (access + refresh tokens)
- Email confirmation on registration
- Forgot password / reset password flow
- Two-Factor Authentication (TOTP via `Otp.NET`) — enable, disable, verify, force-reset
- Account lockout after failed login attempts
- Role-based authorization: `User`, `Admin`, `SuperAdmin`

### 👤 User Profile Management
- Change name, email (with confirmation), password, phone number
- Email change triggers a warning email to the old address + confirmation email to the new one

### 🛡️ Admin Panel
- Change user roles
- Deactivate / reactivate users
- Delete users
- Unlock locked-out accounts

### 📋 Audit Logging
- Every sensitive user action is recorded in an `AuditLog` table
- Domain events fire on state changes: `UserRegistered`, `UserLoggedIn`, `UserPasswordChanged`, `UserRoleChanged`, `UserTwoFactorEnabled`, etc.

---

## 🛠️ Tech Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 9 |
| ORM | Entity Framework Core 9 — SQL Server provider |
| Identity | ASP.NET Core Identity (`Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.x) |
| CQRS / Mediator | MediatR 14 |
| Validation | FluentValidation 12 |
| Object Mapping | AutoMapper 12 |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer` 9.0) |
| Two-Factor Auth | TOTP via `Otp.NET` 1.4.1 |
| Password Hashing | `BCrypt.Net-Next` 4.1.0 |
| Email | MailKit 4.15 — SMTP (defaults to `smtp.gmail.com:587`) |
| File Storage | Local filesystem (`IFileStorageService`) |
| Python Service | HTTP client (`HttpClient`) → external Python RAG/TTS microservice |
| API Docs | Swagger / Swashbuckle 10.1 |
| Architecture Tests | NetArchTest.Rules 1.3.2 |

---

## 📁 Project Structure

```
Doc2Pod.sln
│
├── API/
│   ├── Controllers/
│   │   ├── AuthenticationController.cs   # register, login, logout, confirm-email, forgot/reset password, 2FA verify
│   │   ├── ProfileController.cs          # change name/email/password/phone
│   │   ├── SecurityController.cs         # enable/disable 2FA
│   │   ├── DocumentController.cs         # upload PDF
│   │   ├── PodcastController.cs          # generate podcast, get status
│   │   ├── AdminController.cs            # user management (admin only)
│   │   └── ApIController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
│
├── Application/
│   ├── Features/
│   │   ├── Documents/Commands/UploadDocument/
│   │   ├── Podcasts/
│   │   │   ├── Commands/GeneratePodcast/
│   │   │   ├── Queries/GetPodcast/
│   │   │   └── DTOs/
│   │   └── Users/
│   │       ├── Commands/Authentication/  # Register, Login, Logout, ConfirmEmail, ForgotPassword, ResetPassword, VerifyTwoFactor, ConfirmEmailChange
│   │       ├── Commands/Profile/         # ChangeName, ChangeEmail, ChangePassword, SetPhoneNumber
│   │       ├── Commands/Security/        # Enable2FA, Disable2FA
│   │       ├── Commands/Admin/           # ChangeRole, Deactivate, ReActivate, Delete, UnlockAccount
│   │       └── Queries/                  # GetUserById, GetUserByEmail, GetUsersByRole
│   ├── Common/Behaviors/                 # LoggingBehaviour, ValidationBehaviour (pipeline)
│   └── Interfaces/                       # IEmailService, IFileStorageService, IIdentityService, IPasswordHasher, IPythonRagService, ITokenService
│
├── Domain/
│   ├── Entities/
│   │   ├── Document.cs
│   │   └── Podcast.cs
│   ├── Users/
│   │   ├── User.cs                       # Aggregate root
│   │   ├── UserRole.cs
│   │   ├── ValueObjects/                 # Email, FullName, PhoneNumber, Username
│   │   ├── Events/                       # 16 domain events
│   │   ├── Rules/                        # AccountLockout, PasswordReuse, UniqueEmail, UniqueUsername
│   │   └── Errors/
│   ├── Enums/
│   │   ├── PodcastMode.cs                # Query=1, PageRange=2, Full=3
│   │   └── FileAccessType.cs
│   ├── Settings/                         # ApiSettings, EmailSettings
│   └── SharedKernel/                     # Entity, AggregateRoot, Result<T>, Error, ValueObject, DomainEvent, ...
│
└── Infrastructure/
    ├── Identity/                         # ApplicationUser, IdentityService
    ├── Presistence/
    │   ├── Data/AppDbContext.cs
    │   ├── Configurations/               # Fluent API for Document, Podcast, User, AuditLog
    │   ├── Interceptors/DomainEventInterceptor.cs
    │   └── Entities/AuditLog.cs
    ├── Repositories/                     # UserRepository, DocumentRepository, PodcastRepository, GenericRepository, UnitOfWork
    ├── Services/
    │   ├── TokenService.cs               # JWT access + refresh tokens
    │   ├── TotpService.cs                # TOTP generation & verification
    │   ├── AuditService.cs
    │   ├── EmailService.cs               # MailKit SMTP
    │   ├── FileStorageService.cs
    │   ├── PasswordHasher.cs             # BCrypt
    │   └── PythonService/
    │       ├── PythonRagService.cs       # HTTP client proxy to Python AI backend
    │       └── DTOs/                     # GenerateRequest, GenerateStartResponse, TaskStatusResponse
    └── Migrations/
```

---

## ⚙️ Configuration

The application reads configuration from `appsettings.json` and **User Secrets** (for local development — never commit secrets).

### Required Configuration Keys

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=Doc2Pod;..."
  },

  "Jwt": {
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "SecretKey": "your-secret-key-min-32-chars",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  },

  "EmailSettings": {
    "Email": "your-email@gmail.com",
    "Password": "your-app-password",
    "Host": "smtp.gmail.com",
    "Port": 587
  },

  "ApiSettings": {
    "BaseUrl": "https://your-api-base-url"
  },

  "PythonService": {
    "BaseUrl": "http://localhost:8000"
  }
}
```

> 💡 Use `dotnet user-secrets` for `Jwt:SecretKey`, `EmailSettings:Password`, and `ConnectionStrings:DefaultConnection` in development.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or remote)
- The Python RAG/TTS microservice running and reachable at the URL set in `PythonService:BaseUrl`

### Steps

```bash
# 1. Clone the repository
git clone https://github.com/your-username/Doc2Pod.git
cd Doc2Pod

# 2. Restore packages
dotnet restore

# 3. Set up user secrets (from inside the API project folder)
cd API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Your_Connection_String"
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-at-least-32-characters"
dotnet user-secrets set "EmailSettings:Password" "your-email-app-password"

# 4. Apply database migrations
dotnet ef database update --project ../Infrastructure --startup-project .

# 5. Run the API
dotnet run
```

The API will be available at `http://localhost:5022` by default.  
Swagger UI is accessible at `http://localhost:5022/swagger` in Development mode.

---

## 📡 API Endpoints

### Authentication — `/api/authentication`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/register` | Public | Register a new user |
| POST | `/login` | Public | Login, returns JWT (or 2FA challenge) |
| POST | `/logout` | Required | Logout |
| POST | `/confirm-email` | Public | Confirm email address |
| POST | `/confirm-email-change` | Required | Confirm new email after change request |
| POST | `/forgot-password` | Public | Send password reset email |
| POST | `/reset-password` | Public | Reset password with token |
| POST | `/verify-two-factor` | Public | Complete 2FA login step |

### Profile — `/api/profile`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| PUT | `/change-name` | Required | Update display name |
| PUT | `/change-email` | Required | Request email change (sends confirmation emails) |
| PUT | `/change-password` | Required | Change password |
| PUT | `/set-phone-number` | Required | Set or update phone number |

### Security — `/api/security`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/enable-2fa` | Required | Enable TOTP two-factor authentication |
| POST | `/disable-2fa` | Required | Disable two-factor authentication |

### Documents — `/api/document`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/upload` | Required | Upload a PDF document, returns `documentId` |

### Podcasts — `/api/podcast`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/generate` | Required | Start podcast generation, returns `taskId` |
| GET | `/{podcastId}` | Required | Poll generation status / get result |

### Admin — `/api/admin/users`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| PUT | `/{userId}/role` | Admin/SuperAdmin | Change user role |
| POST | `/{userId}/deactivate` | Admin/SuperAdmin | Deactivate a user |
| POST | `/{userId}/reactivate` | Admin/SuperAdmin | Reactivate a user |
| DELETE | `/{userId}` | Admin/SuperAdmin | Delete a user |
| POST | `/{userId}/unlock` | Admin/SuperAdmin | Unlock a locked-out account |

---

## 🔄 Podcast Generation Flow

```
Client                   .NET API                  Python Service
  │                          │                           │
  │──POST /document/upload──▶│                           │
  │◀── documentId ───────────│                           │
  │                          │                           │
  │──POST /podcast/generate──▶│                          │
  │   { documentId, mode,    │──POST /generate ─────────▶│
  │     topic?, startPage?,  │                           │ (RAG + Egyptian Arabic
  │     endPage? }           │◀── taskId ────────────────│  dialect rewrite + TTS)
  │◀── podcastId ────────────│                           │
  │                          │                           │
  │──GET /podcast/{id} ─────▶│──GET /status/{taskId} ───▶│
  │◀── { status, audioPath } │◀── { status, audioPath } ─│
```

---

## 🧪 Architecture Tests

The project includes `NetArchTest.Rules` for enforcing Clean Architecture constraints (e.g., Domain must not reference Infrastructure, Application must not reference API, etc.).

---

## 📄 License

This project is private. No license has been declared.
