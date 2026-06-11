# Tax Ombud Case Management System — Full Project Generation Prompt
> For use with AI coding agents (Antigravity, Cursor, GitHub Copilot Workspace, etc.)

---

## MISSION

Generate a **production-ready, fully functional** backend API for the **Nigerian Tax Ombud Case Management System**. The system enables taxpayers to lodge complaints against SARS (Nigerian Revenue Service), tracks investigations, manages cases through their full lifecycle, and produces statutory reports.

Build the **complete solution** — every layer, every file, no placeholders, no `// TODO` comments, no stub methods. Every interface must have a concrete implementation. Every service must connect to real infrastructure.

---

## TECH STACK (NON-NEGOTIABLE)

| Layer | Technology |
|---|---|
| Runtime | .NET 10 (latest stable) |
| API Framework | ASP.NET Core 10 — minimal hosting model |
| ORM | Entity Framework Core 10 |
| Database | Microsoft SQL Server (MSSQL) — code-first migrations |
| Authentication | JWT Bearer tokens + Refresh tokens (stored in DB) |
| MFA | TOTP via `OtpNet` library |
| Password Hashing | `BCrypt.Net-Next` |
| Validation | FluentValidation |
| Mapping | Mapster (preferred over AutoMapper for performance) |
| Logging | Serilog → structured JSON logs → Console + File sinks |
| API Docs | Swashbuckle (Swagger UI + OpenAPI 3.0) |
| Health Checks | `AspNetCore.HealthChecks.SqlServer` |
| Email | `MailKit` / `MimeKit` |
| File Storage | Local disk (abstracted behind `IStorageService` — swappable to Azure Blob) |
| Background Jobs | `Hangfire` with MSSQL persistence |
| Testing | xUnit + Moq + FluentAssertions + `Microsoft.AspNetCore.Mvc.Testing` |
| Containerisation | `Dockerfile` + `docker-compose.yml` (API + SQL Server) |

---

## SOLUTION STRUCTURE — CLEAN ARCHITECTURE

Use the following project layout strictly. Each project targets `net9.0`.

```
TaxOmbud/
├── src/
│   ├── TaxOmbud.Domain/               # Enterprise business rules — zero dependencies
│   ├── TaxOmbud.Application/          # Use cases, interfaces, DTOs, validators
│   ├── TaxOmbud.Infrastructure/       # EF Core, MSSQL, email, storage, Hangfire
│   └── TaxOmbud.API/                  # ASP.NET Core — controllers, middleware, Program.cs
├── tests/
│   ├── TaxOmbud.Domain.Tests/
│   ├── TaxOmbud.Application.Tests/
│   └── TaxOmbud.API.IntegrationTests/
├── TaxOmbud.sln
├── docker-compose.yml
├── Dockerfile
└── README.md
```

### Dependency Rule
The dependency arrows flow **inward only**:
```
API → Application → Domain
Infrastructure → Application → Domain
```
Domain has **zero** project references. Application references only Domain.
Infrastructure and API reference Application (never each other directly for business logic).

---

## LAYER SPECIFICATIONS

### 1. `TaxOmbud.Domain`

Contains **only** pure C# — no NuGet packages except `MediatR.Contracts` for domain events.

**Folder structure:**
```
TaxOmbud.Domain/
├── Common/
│   ├── BaseEntity.cs              # Id (Guid), CreatedAt, UpdatedAt, ISoftDelete
│   ├── BaseAuditableEntity.cs     # + CreatedBy, UpdatedBy (UserId)
│   ├── IDomainEvent.cs
│   ├── IHasDomainEvents.cs
│   └── PagedResult.cs
├── Entities/
│   ├── Identity/
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── Permission.cs
│   │   ├── RolePermission.cs
│   │   ├── UserRole.cs
│   │   ├── RefreshToken.cs
│   │   └── MfaToken.cs
│   ├── Taxpayers/
│   │   ├── Taxpayer.cs
│   │   ├── TaxpayerAddress.cs
│   │   └── TaxpayerContactDetail.cs
│   ├── Officers/
│   │   ├── Officer.cs
│   │   └── OfficerPerformanceRecord.cs
│   ├── Complaints/
│   │   ├── Complaint.cs
│   │   ├── ComplaintStatusHistory.cs
│   │   ├── ComplaintNote.cs
│   │   └── ComplaintLink.cs
│   ├── Cases/
│   │   ├── Case.cs
│   │   ├── CaseStatusHistory.cs
│   │   ├── CaseMilestone.cs
│   │   ├── CaseFinding.cs
│   │   ├── CaseRecommendation.cs
│   │   └── CaseCommunicationLog.cs
│   ├── Documents/
│   │   ├── Document.cs
│   │   └── DocumentVersion.cs
│   ├── Communications/
│   │   ├── Communication.cs
│   │   └── CommunicationTemplate.cs
│   ├── Appeals/
│   │   ├── Appeal.cs
│   │   └── AppealStatusHistory.cs
│   ├── Appointments/
│   │   └── Appointment.cs
│   ├── Notifications/
│   │   ├── Notification.cs
│   │   └── NotificationPreference.cs
│   └── System/
│       ├── AuditLog.cs
│       ├── SystemSetting.cs
│       ├── FeatureFlag.cs
│       ├── WebhookSubscription.cs
│       ├── WebhookDelivery.cs
│       └── ScheduledReport.cs
├── Enums/
│   ├── ComplaintStatus.cs         # Draft, Submitted, UnderReview, Escalated, Resolved, Closed, Withdrawn
│   ├── CaseStatus.cs              # Open, InProgress, PendingSarsResponse, UnderReview, Closed
│   ├── AppealStatus.cs            # Submitted, UnderReview, Upheld, Dismissed
│   ├── AppointmentStatus.cs       # Scheduled, Confirmed, Completed, Cancelled
│   ├── DocumentEntityType.cs      # Complaint, Case, Appeal
│   ├── CommunicationDirection.cs  # Inbound, Outbound
│   ├── UserStatus.cs              # Active, Inactive, Locked
│   └── TaxpayerType.cs            # Individual, Company, Trust, Partnership
├── Events/
│   ├── Complaints/
│   │   ├── ComplaintSubmittedEvent.cs
│   │   ├── ComplaintStatusChangedEvent.cs
│   │   └── ComplaintEscalatedEvent.cs
│   ├── Cases/
│   │   ├── CaseOpenedEvent.cs
│   │   ├── CaseAssignedEvent.cs
│   │   └── CaseClosedEvent.cs
│   └── Appeals/
│       └── AppealSubmittedEvent.cs
├── Exceptions/
│   ├── DomainException.cs
│   ├── NotFoundException.cs
│   ├── ForbiddenException.cs
│   ├── ConflictException.cs
│   └── ValidationException.cs
└── ValueObjects/
    ├── TaxIdentificationNumber.cs  # Validated SA TIN format
    ├── ReferenceNumber.cs          # e.g. "TO-2024-000001"
    └── Email.cs
```

**Entity requirements:**
- All entities inherit `BaseAuditableEntity`
- Entities with status workflows (`Complaint`, `Case`, `Appeal`) must expose **domain methods** that encapsulate state transitions and raise domain events. Do NOT allow direct property mutation of `Status` from outside the entity. Example:
  ```csharp
  // Complaint.cs — good
  public Result Escalate(string reason, Guid escalatedByUserId)
  {
      if (Status != ComplaintStatus.UnderReview)
          return Result.Failure("Only complaints under review can be escalated.");
      Status = ComplaintStatus.Escalated;
      AddDomainEvent(new ComplaintEscalatedEvent(Id, reason, escalatedByUserId));
      return Result.Success();
  }
  ```
- `Complaint.ReferenceNumber` must be generated as `TO-{YYYY}-{6-digit-sequence}` and be unique
- `Case.CaseNumber` must be generated as `CASE-{YYYY}-{6-digit-sequence}`
- All money/amount fields use `decimal` with `precision(18,2)`

---

### 2. `TaxOmbud.Application`

References: `TaxOmbud.Domain`, `MediatR`, `FluentValidation`, `Mapster`

**Folder structure:**
```
TaxOmbud.Application/
├── Common/
│   ├── Behaviours/
│   │   ├── ValidationBehaviour.cs       # MediatR pipeline — runs FluentValidation
│   │   ├── LoggingBehaviour.cs          # Logs every command/query with timing
│   │   ├── AuthorizationBehaviour.cs    # Attribute-based command-level authorization
│   │   └── AuditBehaviour.cs            # Auto-creates AuditLog entries for commands
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs     # EF DbContext abstraction
│   │   ├── ICurrentUserService.cs       # UserId, Roles, IsAuthenticated
│   │   ├── IDateTimeService.cs          # Abstracted UtcNow for testability
│   │   ├── IStorageService.cs           # Upload, Download, Delete blobs
│   │   ├── IEmailService.cs             # SendAsync(EmailMessage)
│   │   ├── ITokenService.cs             # GenerateJwt, GenerateRefreshToken
│   │   ├── IAuditService.cs             # LogAsync(AuditEntry)
│   │   └── IReferenceNumberService.cs   # GenerateComplaintRef, GenerateCaseNumber
│   ├── Models/
│   │   ├── Result.cs                    # Result<T> — success/failure with error list
│   │   ├── PagedResult.cs
│   │   └── EmailMessage.cs
│   └── Mappings/
│       └── MappingConfig.cs             # Mapster global configuration
├── Features/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── Login/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   ├── LoginCommandValidator.cs
│   │   │   │   └── LoginResponse.cs
│   │   │   ├── RefreshToken/
│   │   │   ├── Logout/
│   │   │   ├── ForgotPassword/
│   │   │   ├── ResetPassword/
│   │   │   ├── ChangePassword/
│   │   │   ├── VerifyEmail/
│   │   │   ├── SetupMfa/
│   │   │   ├── VerifyMfa/
│   │   │   └── DisableMfa/
│   ├── Complaints/
│   │   ├── Commands/
│   │   │   ├── CreateComplaint/
│   │   │   │   ├── CreateComplaintCommand.cs
│   │   │   │   ├── CreateComplaintCommandHandler.cs
│   │   │   │   └── CreateComplaintCommandValidator.cs
│   │   │   ├── UpdateComplaint/
│   │   │   ├── AssignComplaint/
│   │   │   ├── EscalateComplaint/
│   │   │   ├── CloseComplaint/
│   │   │   ├── ReopenComplaint/
│   │   │   ├── UpdateComplaintStatus/
│   │   │   ├── AddComplaintNote/
│   │   │   ├── LinkComplaints/
│   │   │   ├── UploadComplaintDocument/
│   │   │   └── DeleteComplaint/
│   │   └── Queries/
│   │       ├── GetComplaints/
│   │       ├── GetComplaintById/
│   │       ├── GetComplaintByReferenceNumber/
│   │       ├── GetComplaintTimeline/
│   │       ├── GetComplaintNotes/
│   │       ├── GetComplaintDocuments/
│   │       ├── GetRelatedComplaints/
│   │       └── GetMyComplaints/
│   ├── Cases/
│   │   ├── Commands/
│   │   │   ├── CreateCase/
│   │   │   ├── UpdateCase/
│   │   │   ├── AssignCase/
│   │   │   ├── UpdateCaseStatus/
│   │   │   ├── CloseCase/
│   │   │   ├── AddCaseFinding/
│   │   │   ├── UpdateCaseFinding/
│   │   │   ├── AddCaseRecommendation/
│   │   │   ├── LogCaseCommunication/
│   │   │   ├── UploadCaseDocument/
│   │   │   └── CompleteCaseMilestone/
│   │   └── Queries/
│   │       ├── GetCases/
│   │       ├── GetCaseById/
│   │       ├── GetCaseFindings/
│   │       ├── GetCaseRecommendations/
│   │       ├── GetCaseCommunications/
│   │       ├── GetCaseDocuments/
│   │       ├── GetCaseMilestones/
│   │       ├── GetMyCases/
│   │       └── GetOverdueCases/
│   ├── Taxpayers/           # (same Commands/Queries pattern)
│   ├── Officers/
│   ├── Documents/
│   ├── Communications/
│   ├── Appeals/
│   ├── Appointments/
│   ├── Notifications/
│   ├── Reports/
│   ├── Search/
│   ├── Roles/
│   ├── Lookups/
│   ├── AuditLogs/
│   ├── Settings/
│   └── Webhooks/
└── DependencyInjection.cs   # AddApplication(services) extension
```

**Handler requirements:**
- Every `Command` and `Query` is a MediatR `IRequest<Result<T>>`
- Every `CommandHandler`/`QueryHandler` implements `IRequestHandler<TRequest, Result<T>>`
- Handlers must never access `HttpContext` directly — use `ICurrentUserService`
- All database reads in Query handlers must use `.AsNoTracking()`
- Handlers must publish domain events via MediatR after saving (`await _mediator.Publish(domainEvent)`)
- `ValidationBehaviour` must run before every handler — reject with `Result.Failure` on invalid input

---

### 3. `TaxOmbud.Infrastructure`

References: `TaxOmbud.Application`, EF Core, Serilog, MailKit, Hangfire, BCrypt, OtpNet

**Folder structure:**
```
TaxOmbud.Infrastructure/
├── Persistence/
│   ├── TaxOmbudDbContext.cs
│   ├── Configurations/              # One IEntityTypeConfiguration<T> per entity
│   │   ├── UserConfiguration.cs
│   │   ├── ComplaintConfiguration.cs
│   │   └── ... (all 40+ entities)
│   ├── Migrations/                  # EF Core generated migrations
│   ├── Repositories/                # Generic + specific repos (optional, only if needed)
│   └── Seeders/
│       ├── RoleSeeder.cs            # Seeds Admin, Supervisor, Officer, Taxpayer roles
│       ├── PermissionSeeder.cs
│       ├── LookupSeeder.cs          # Provinces, SarsOffices, ComplaintTypes, etc.
│       └── AdminUserSeeder.cs       # Creates default admin from appsettings
├── Identity/
│   ├── TokenService.cs              # JWT generation, refresh token management
│   ├── PasswordService.cs           # BCrypt hashing/verification
│   └── MfaService.cs                # TOTP via OtpNet
├── Services/
│   ├── CurrentUserService.cs        # Reads ClaimsPrincipal from IHttpContextAccessor
│   ├── DateTimeService.cs
│   ├── EmailService.cs              # MailKit SMTP implementation
│   ├── StorageService.cs            # Local file system (IStorageService)
│   ├── ReferenceNumberService.cs    # DB-sequenced reference number generation
│   ├── AuditService.cs
│   └── WebhookDispatchService.cs    # HTTP delivery with retry and HMAC signing
├── BackgroundJobs/
│   ├── HangfireJobScheduler.cs
│   ├── SendScheduledReportJob.cs
│   ├── SlaBreachNotificationJob.cs  # Runs daily — flags overdue cases
│   └── WebhookRetryJob.cs
└── DependencyInjection.cs           # AddInfrastructure(services, configuration) extension
```

**EF Core configuration rules:**
- Every entity gets its own `IEntityTypeConfiguration<T>` class in `Configurations/`
- All string columns must have explicit `HasMaxLength()`
- All `enum` columns stored as `int` with a corresponding comment
- Global query filters on soft-deleted entities
- `ValueConverter` for `TaxIdentificationNumber`, `ReferenceNumber` value objects
- All navigation properties explicitly configured (no EF convention reliance for FKs)
- Required indexes:
  - `Complaints`: unique on `ReferenceNumber`, composite on `(Status, CreatedAt)`, on `TaxpayerId`
  - `Cases`: unique on `CaseNumber`, composite on `(Status, AssignedOfficerId)`
  - `Taxpayers`: unique on `TaxIdentificationNumber`, on `Email`
  - `Users`: unique on `Email`
  - `AuditLogs`: composite on `(EntityType, EntityId)`, on `CreatedAt`
  - `RefreshTokens`: unique on `Token`
  - `Notifications`: composite on `(UserId, IsRead)`

---

### 4. `TaxOmbud.API`

References: `TaxOmbud.Application`, `TaxOmbud.Infrastructure`

**Folder structure:**
```
TaxOmbud.API/
├── Controllers/
│   ├── v1/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── TaxpayersController.cs
│   │   ├── OfficersController.cs
│   │   ├── ComplaintsController.cs
│   │   ├── CasesController.cs
│   │   ├── DocumentsController.cs
│   │   ├── CommunicationsController.cs
│   │   ├── AppealsController.cs
│   │   ├── AppointmentsController.cs
│   │   ├── NotificationsController.cs
│   │   ├── ReportsController.cs
│   │   ├── SearchController.cs
│   │   ├── LookupsController.cs
│   │   ├── AuditLogsController.cs
│   │   ├── RolesController.cs
│   │   ├── SettingsController.cs
│   │   ├── WebhooksController.cs
│   │   └── HealthController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs   # Global — maps exceptions to RFC 7807 ProblemDetails
│   ├── RequestLoggingMiddleware.cs      # Logs method, path, status, duration
│   ├── RateLimitingMiddleware.cs        # Per-IP + per-user rate limits
│   └── SecurityHeadersMiddleware.cs    # Adds HSTS, X-Frame-Options, CSP, etc.
├── Filters/
│   └── ApiKeyAuthFilter.cs             # For webhook receiver endpoints
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   ├── WebApplicationExtensions.cs
│   └── ClaimsPrincipalExtensions.cs    # GetUserId(), GetRoles(), IsInRole() helpers
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

**Controller requirements:**
- All controllers inherit `ApiControllerBase : ControllerBase` (a shared base that extracts `UserId` from claims and maps `Result<T>` to appropriate HTTP responses)
- Route prefix: `[Route("api/v1/[controller]")]`
- Each action calls a MediatR command or query — no business logic inside controllers
- Return types: `ActionResult<T>` — never raw `IActionResult` alone
- Use `ProducesResponseType` attributes on every action
- Document every action with `/// <summary>` XML comments

**`ApiControllerBase` pattern:**
```csharp
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender _mediator = null!;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected ActionResult<T> HandleResult<T>(Result<T> result) => result switch
    {
        { IsSuccess: true, Value: not null } => Ok(result.Value),
        { IsSuccess: true }                  => NoContent(),
        { IsNotFound: true }                 => NotFound(result.Errors),
        { IsForbidden: true }                => Forbid(),
        { IsConflict: true }                 => Conflict(result.Errors),
        _                                    => BadRequest(result.Errors)
    };
}
```

---

## ALL 181 ENDPOINTS TO IMPLEMENT

Implement every endpoint in the table below. For each: create the Controller action, a MediatR Command or Query, a Handler, a Validator (for commands), and the appropriate DTO/response types.

### AUTH (10)
| Method | Route | Access | Notes |
|--------|-------|--------|-------|
| POST | `/api/v1/auth/login` | Public | Returns `AccessToken`, `RefreshToken`, `ExpiresIn`, `User` |
| POST | `/api/v1/auth/refresh` | Public | Validates refresh token, rotates it |
| POST | `/api/v1/auth/logout` | Authenticated | Revokes the supplied refresh token |
| POST | `/api/v1/auth/forgot-password` | Public | Always returns 204 (no enumeration) |
| POST | `/api/v1/auth/reset-password` | Public | Token valid for 1 hour |
| POST | `/api/v1/auth/change-password` | Authenticated | Requires current password |
| POST | `/api/v1/auth/verify-email` | Public | Token valid for 24 hours |
| POST | `/api/v1/auth/mfa/setup` | Authenticated | Returns TOTP URI + backup codes |
| POST | `/api/v1/auth/mfa/verify` | Authenticated | Enables MFA on account |
| POST | `/api/v1/auth/mfa/disable` | Authenticated | Requires valid TOTP code |

### USERS (11)
| Method | Route | Roles | Notes |
|--------|-------|-------|-------|
| GET | `/api/v1/users` | Admin | Filter: status, role, search string |
| GET | `/api/v1/users/{id}` | Admin | |
| POST | `/api/v1/users` | Admin | Sends welcome/activation email |
| PUT | `/api/v1/users/{id}` | Admin | |
| PATCH | `/api/v1/users/{id}/activate` | Admin | |
| PATCH | `/api/v1/users/{id}/deactivate` | Admin | Cannot deactivate own account |
| PATCH | `/api/v1/users/{id}/roles` | Admin | |
| POST | `/api/v1/users/{id}/unlock` | Admin | Resets lockout counter |
| GET | `/api/v1/users/me` | Authenticated | |
| PUT | `/api/v1/users/me` | Authenticated | |
| GET | `/api/v1/users/{id}/audit-log` | Admin | Paginated |

### TAXPAYERS (9)
| Method | Route | Roles | Notes |
|--------|-------|-------|-------|
| GET | `/api/v1/taxpayers` | Officer,Supervisor,Admin | Full-text search |
| GET | `/api/v1/taxpayers/{id}` | Authenticated | Taxpayer can only see own |
| GET | `/api/v1/taxpayers/tin/{tin}` | Officer,Supervisor,Admin | |
| POST | `/api/v1/taxpayers` | Public | Self-registration; sends verify email |
| PUT | `/api/v1/taxpayers/{id}` | Authenticated | Taxpayer can only update own |
| PATCH | `/api/v1/taxpayers/{id}/verify` | Officer,Admin | Sets `IsVerified = true` |
| PATCH | `/api/v1/taxpayers/{id}/deactivate` | Admin | |
| GET | `/api/v1/taxpayers/{id}/complaints` | Officer,Supervisor,Admin | |
| GET | `/api/v1/taxpayers/me` | Taxpayer | |

### OFFICERS (5)
| Method | Route | Roles | Notes |
|--------|-------|-------|-------|
| GET | `/api/v1/officers` | Supervisor,Admin | |
| GET | `/api/v1/officers/{id}` | Supervisor,Admin | Includes active case count |
| GET | `/api/v1/officers/{id}/caseload` | Supervisor,Admin | Active cases only |
| GET | `/api/v1/officers/{id}/performance` | Supervisor,Admin | Date-range filtered |
| GET | `/api/v1/officers/available` | Supervisor,Admin | Officers with < max caseload |

### COMPLAINTS (18)
| Method | Route | Roles | Notes |
|--------|-------|-------|-------|
| GET | `/api/v1/complaints` | Officer,Supervisor,Admin | Filter: status, type, officer, date range, province |
| GET | `/api/v1/complaints/{id}` | Authenticated | Taxpayer sees only own |
| GET | `/api/v1/complaints/reference/{refNo}` | Public | Returns status + summary only |
| POST | `/api/v1/complaints` | Public | Generates `TO-YYYY-NNNNNN` ref number |
| PUT | `/api/v1/complaints/{id}` | Authenticated | Only in `Draft` status |
| DELETE | `/api/v1/complaints/{id}` | Supervisor,Admin | Soft delete |
| PATCH | `/api/v1/complaints/{id}/status` | Officer,Supervisor,Admin | Validates allowed transitions |
| PATCH | `/api/v1/complaints/{id}/assign` | Supervisor,Admin | Sets assigned officer |
| PATCH | `/api/v1/complaints/{id}/escalate` | Officer,Supervisor,Admin | |
| PATCH | `/api/v1/complaints/{id}/close` | Supervisor,Admin | Requires closure reason |
| PATCH | `/api/v1/complaints/{id}/reopen` | Supervisor,Admin | |
| GET | `/api/v1/complaints/{id}/timeline` | Authenticated | Status history + timestamps |
| GET | `/api/v1/complaints/{id}/notes` | Officer,Supervisor,Admin | Internal only |
| POST | `/api/v1/complaints/{id}/notes` | Officer,Supervisor,Admin | |
| GET | `/api/v1/complaints/{id}/documents` | Authenticated | |
| POST | `/api/v1/complaints/{id}/documents` | Authenticated | Max 10MB per file; PDF/DOC/DOCX/XLSX/JPG/PNG |
| GET | `/api/v1/complaints/{id}/related` | Officer,Supervisor,Admin | |
| POST | `/api/v1/complaints/{id}/link` | Officer,Supervisor,Admin | |
| GET | `/api/v1/complaints/my` | Taxpayer | |

### CASES (20)
| Method | Route | Roles | Notes |
|--------|-------|-------|-------|
| GET | `/api/v1/cases` | Officer,Supervisor,Admin | Filter: status, officer, date range |
| GET | `/api/v1/cases/{id}` | Officer,Supervisor,Admin | Full detail |
| POST | `/api/v1/cases` | Officer,Supervisor,Admin | Links to `ComplaintId` |
| PUT | `/api/v1/cases/{id}` | Officer,Supervisor,Admin | |
| PATCH | `/api/v1/cases/{id}/assign` | Supervisor,Admin | |
| PATCH | `/api/v1/cases/{id}/status` | Officer,Supervisor,Admin | |
| PATCH | `/api/v1/cases/{id}/close` | Supervisor,Admin | Requires outcome + finding |
| GET | `/api/v1/cases/{id}/findings` | Officer,Supervisor,Admin | |
| POST | `/api/v1/cases/{id}/findings` | Officer,Supervisor,Admin | |
| PUT | `/api/v1/cases/{id}/findings/{findingId}` | Officer,Supervisor,Admin | |
| GET | `/api/v1/cases/{id}/recommendations` | Officer,Supervisor,Admin | |
| POST | `/api/v1/cases/{id}/recommendations` | Supervisor,Admin | |
| GET | `/api/v1/cases/{id}/communications` | Officer,Supervisor,Admin | |
| POST | `/api/v1/cases/{id}/communications` | Officer,Supervisor,Admin | |
| GET | `/api/v1/cases/{id}/documents` | Officer,Supervisor,Admin | |
| POST | `/api/v1/cases/{id}/documents` | Officer,Supervisor,Admin | |
| GET | `/api/v1/cases/{id}/milestones` | Officer,Supervisor,Admin | |
| PATCH | `/api/v1/cases/{id}/milestones/{milestoneId}` | Officer,Supervisor,Admin | |
| GET | `/api/v1/cases/my` | Officer | |
| GET | `/api/v1/cases/overdue` | Officer,Supervisor,Admin | Past SLA deadline |

### DOCUMENTS (8)
| Method | Route | Roles | Notes |
|--------|-------|-------|-------|
| GET | `/api/v1/documents` | Officer,Supervisor,Admin | Filter by entity type/id |
| GET | `/api/v1/documents/{id}` | Authenticated | |
| GET | `/api/v1/documents/{id}/download` | Authenticated | Streams file; checks ownership |
| POST | `/api/v1/documents` | Authenticated | multipart/form-data |
| DELETE | `/api/v1/documents/{id}` | Officer,Supervisor,Admin | Soft delete |
| GET | `/api/v1/documents/{id}/versions` | Authenticated | |
| POST | `/api/v1/documents/{id}/versions` | Authenticated | New version replaces current |
| PATCH | `/api/v1/documents/{id}/classify` | Officer,Supervisor,Admin | |

### COMMUNICATIONS (7)
| Method | Route | Roles |
|--------|-------|-------|
| GET | `/api/v1/communications` | Officer,Supervisor,Admin |
| GET | `/api/v1/communications/{id}` | Officer,Supervisor,Admin |
| POST | `/api/v1/communications` | Officer,Supervisor,Admin |
| POST | `/api/v1/communications/{id}/send` | Officer,Supervisor,Admin |
| PATCH | `/api/v1/communications/{id}/acknowledge` | Officer,Supervisor,Admin |
| GET | `/api/v1/communications/templates` | Officer,Supervisor,Admin |
| POST | `/api/v1/communications/templates/{templateId}/render` | Officer,Supervisor,Admin |

### APPEALS (8)
| Method | Route | Roles |
|--------|-------|-------|
| GET | `/api/v1/appeals` | Officer,Supervisor,Admin |
| GET | `/api/v1/appeals/{id}` | Authenticated |
| POST | `/api/v1/appeals` | Authenticated |
| PATCH | `/api/v1/appeals/{id}/review` | Supervisor,Admin |
| PATCH | `/api/v1/appeals/{id}/uphold` | Supervisor,Admin |
| PATCH | `/api/v1/appeals/{id}/dismiss` | Supervisor,Admin |
| GET | `/api/v1/appeals/{id}/documents` | Authenticated |
| POST | `/api/v1/appeals/{id}/documents` | Authenticated |

### APPOINTMENTS (9)
| Method | Route | Roles |
|--------|-------|-------|
| GET | `/api/v1/appointments` | Authenticated |
| GET | `/api/v1/appointments/{id}` | Authenticated |
| POST | `/api/v1/appointments` | Officer,Supervisor,Admin |
| PUT | `/api/v1/appointments/{id}` | Officer,Supervisor,Admin |
| PATCH | `/api/v1/appointments/{id}/confirm` | Authenticated |
| PATCH | `/api/v1/appointments/{id}/cancel` | Authenticated |
| PATCH | `/api/v1/appointments/{id}/complete` | Officer,Supervisor,Admin |
| GET | `/api/v1/appointments/availability` | Authenticated |
| GET | `/api/v1/appointments/calendar` | Authenticated |

### NOTIFICATIONS (7)
| Method | Route | Roles |
|--------|-------|-------|
| GET | `/api/v1/notifications` | Authenticated |
| GET | `/api/v1/notifications/unread-count` | Authenticated |
| PATCH | `/api/v1/notifications/{id}/read` | Authenticated |
| PATCH | `/api/v1/notifications/read-all` | Authenticated |
| DELETE | `/api/v1/notifications/{id}` | Authenticated |
| GET | `/api/v1/notifications/preferences` | Authenticated |
| PUT | `/api/v1/notifications/preferences` | Authenticated |

### REPORTS (12)
| Method | Route | Roles |
|--------|-------|-------|
| GET | `/api/v1/reports/dashboard` | Officer,Supervisor,Admin |
| GET | `/api/v1/reports/complaints/summary` | Officer,Supervisor,Admin |
| GET | `/api/v1/reports/complaints/by-type` | Officer,Supervisor,Admin |
| GET | `/api/v1/reports/complaints/by-region` | Officer,Supervisor,Admin |
| GET | `/api/v1/reports/cases/resolution-time` | Officer,Supervisor,Admin |
| GET | `/api/v1/reports/cases/outcomes` | Officer,Supervisor,Admin |
| GET | `/api/v1/reports/officers/performance` | Supervisor,Admin |
| GET | `/api/v1/reports/annual` | Supervisor,Admin |
| POST | `/api/v1/reports/export` | Officer,Supervisor,Admin |
| GET | `/api/v1/reports/scheduled` | Admin |
| POST | `/api/v1/reports/scheduled` | Admin |
| DELETE | `/api/v1/reports/scheduled/{id}` | Admin |

### SEARCH (6)
| Method | Route | Notes |
|--------|-------|-------|
| GET | `/api/v1/search?q=` | Global — Officers+ |
| GET | `/api/v1/search/complaints?q=` | |
| GET | `/api/v1/search/cases?q=` | |
| GET | `/api/v1/search/taxpayers?q=` | |
| GET | `/api/v1/search/documents?q=` | |
| GET | `/api/v1/search/suggestions?q=` | Autocomplete, max 10 results |

### LOOKUPS (16), AUDIT LOGS (4), ROLES (8), SETTINGS (11), WEBHOOKS (8), HEALTH (4)
Implement all as specified in the endpoint reference above.

---

## SECURITY REQUIREMENTS

Implement all of the following — no exceptions:

### Authentication & Session
- JWT access tokens: 15-minute expiry, RS256 signing (asymmetric keys from config)
- Refresh tokens: 7-day expiry, single-use with rotation (each refresh issues a new token and revokes the old)
- Refresh token stored hashed in DB (SHA-256), not plaintext
- Account lockout after 5 consecutive failed login attempts — 15-minute lockout
- TOTP MFA support (RFC 6238) via `OtpNet`
- Backup codes: 8 single-use codes generated on MFA setup, stored as BCrypt hashes

### Authorization
- Role hierarchy: `Taxpayer < Officer < Supervisor < Admin`
- Resource ownership checks: taxpayers can only access their own complaints, documents, and appointments. Enforce at the handler level, not just at the controller level.
- `ICurrentUserService` must be used in handlers — never read `HttpContext` directly in Application layer

### Input Validation & Sanitization
- FluentValidation on every command; return `422 Unprocessable Entity` on failure
- Sanitize all free-text fields (strip dangerous HTML/script content)
- File upload validation: check magic bytes (not just extension), max size 10MB per file, allowed types: PDF, DOC, DOCX, XLS, XLSX, PNG, JPG
- TIN format validation per SA SARS format rules

### Transport & Headers
- Enforce HTTPS; reject HTTP in production
- `SecurityHeadersMiddleware` must set:
  - `Strict-Transport-Security: max-age=31536000; includeSubDomains`
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Content-Security-Policy: default-src 'self'`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy: geolocation=(), microphone=()`

### Rate Limiting
- Global: 1000 requests/minute per IP
- Auth endpoints: 10 requests/minute per IP (brute-force protection)
- File upload: 20 requests/minute per user
- Use `System.Threading.RateLimiting` (built-in .NET 7+ — no third-party package needed)

### Audit Trail
- `AuditBehaviour` MediatR pipeline step: automatically creates `AuditLog` entries for every state-changing command
- Log: `EntityType`, `EntityId`, `Action`, `OldValues` (JSON), `NewValues` (JSON), `UserId`, `IPAddress`, `UserAgent`, `Timestamp`
- `AuditLog` table is append-only: no update or delete endpoints exposed

### Secrets & Configuration
- All secrets (`Jwt:SecretKey`, connection strings, SMTP credentials) read from environment variables or `appsettings.{Environment}.json` — never hardcoded
- `appsettings.Development.json` contains safe localhost defaults only
- Production config via environment variables (12-factor app)

### SQL Injection
- Use parameterised queries exclusively (EF Core LINQ — no raw string interpolation in `FromSqlRaw`)
- Any raw SQL must use `FromSqlRaw` with `SqlParameter` objects

---

## DATABASE REQUIREMENTS

### MSSQL-Specific
- Connection resiliency: `EnableRetryOnFailure(5, TimeSpan.FromSeconds(10))`
- Command timeout: 60 seconds
- Use `newsequentialid()` as the default value for all `GUID` primary keys (better clustered index performance)
- All `datetime` columns use `datetime2(7)` — never `datetime`
- Collation: `Latin1_General_CI_AI` on text columns used for search

### Migrations
- Generate all EF Core migrations; they must apply cleanly on a fresh MSSQL instance
- Include a `DatabaseSeeder` that runs after migration on first startup and seeds:
  - All 4 roles with their permission assignments
  - All lookup tables (Provinces, SarsOffices, ComplaintTypes, TaxTypes, etc.)
  - One default Admin user (credentials from `appsettings.json:DefaultAdmin`)

### Indexes
Create all indexes listed in the Infrastructure section above, plus:
- Full-text search index on `Complaints(Description)`, `Cases(Summary)`, `Taxpayers(FirstName, LastName, Email)` via SQL Server `CONTAINS` / `FREETEXT`

---

## BACKGROUND JOBS (HANGFIRE)

Implement the following recurring jobs:

| Job | Schedule | Description |
|-----|----------|-------------|
| `SlaBreachNotificationJob` | Daily at 06:00 | Query all open cases past their `DueDate`; create `Notification` records for assigned officer and supervisor; fire webhook event `case.sla_breached` |
| `SendScheduledReportJob` | Per schedule config | Execute each active `ScheduledReport`; email as CSV/Excel attachment |
| `WebhookRetryJob` | Every 5 minutes | Retry failed `WebhookDelivery` records (up to 5 attempts with exponential backoff) |
| `CleanupExpiredTokensJob` | Daily at 02:00 | Delete expired `RefreshTokens` and `MfaTokens` |
| `NotificationCleanupJob` | Weekly | Delete read notifications older than 90 days |

---

## WEBHOOK SYSTEM

- `WebhookSubscription` stores: `Url`, `Secret` (for HMAC signing), `EventTypes` (comma-separated), `IsActive`
- On domain event publish, `WebhookDispatchService` queries active subscriptions for that event type
- Payload: `{ event: "complaint.submitted", timestamp: "...", data: { ... } }`
- HMAC-SHA256 signature in `X-TaxOmbud-Signature` header: `sha256=<hex>`
- Max payload 1MB; delivery timeout 10 seconds
- Record every attempt in `WebhookDelivery`

**Event types to support:**
`complaint.submitted`, `complaint.assigned`, `complaint.escalated`, `complaint.closed`,
`case.opened`, `case.assigned`, `case.closed`, `case.sla_breached`,
`appeal.submitted`, `appeal.upheld`, `appeal.dismissed`,
`taxpayer.registered`, `taxpayer.verified`

---

## TESTING REQUIREMENTS

### Unit Tests (`TaxOmbud.Application.Tests`, `TaxOmbud.Domain.Tests`)
- Test every Command handler and Query handler in isolation (mock `IApplicationDbContext`, `ICurrentUserService`, `IEmailService`)
- Test every domain entity's state transition methods (e.g. `Complaint.Escalate()`, `Case.Close()`)
- Test every FluentValidation validator
- Aim for ≥ 80% line coverage on Application and Domain layers

### Integration Tests (`TaxOmbud.API.IntegrationTests`)
- Use `WebApplicationFactory<Program>` with a real SQL LocalDB or testcontainers MSSQL instance
- Write integration tests for at minimum:
  - Full complaint lifecycle: submit → assign → escalate → close
  - Full case lifecycle: open → assign → finding added → closed
  - Auth flow: register → verify email → login → refresh → logout
  - Document upload and download
  - Rate limiting enforcement on auth endpoints

---

## CONFIGURATION FILES

### `appsettings.json` structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaxOmbud;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "TaxOmbud.API",
    "Audience": "TaxOmbud.Client",
    "SecretKey": "REPLACE_WITH_ENV_VAR_IN_PRODUCTION",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Email": {
    "Host": "smtp.mailserver.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "",
    "Password": "",
    "FromAddress": "noreply@taxombud.gov.za",
    "FromName": "Tax Ombud System"
  },
  "Storage": {
    "BasePath": "./uploads",
    "MaxFileSizeMb": 10,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png"]
  },
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "RequireAuth": true
  },
  "DefaultAdmin": {
    "Email": "admin@taxombud.gov.za",
    "Password": "REPLACE_WITH_ENV_VAR_IN_PRODUCTION",
    "FirstName": "System",
    "LastName": "Administrator"
  },
  "RateLimiting": {
    "GlobalRequestsPerMinute": 1000,
    "AuthRequestsPerMinute": 10,
    "UploadRequestsPerMinute": 20
  },
  "Sla": {
    "ComplaintAcknowledgementDays": 2,
    "CaseResolutionDays": 90,
    "AppealResolutionDays": 30
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://portal.taxombud.gov.za"]
  }
}
```

---

## DOCKER SETUP

### `Dockerfile`
Multi-stage build:
1. `sdk:9.0` stage — restore, build, publish
2. `aspnet:9.0` runtime stage — copy published output, expose port 8080, run as non-root user

### `docker-compose.yml`
Services:
- `api` — the built .NET image, `ports: 8080:8080`, env vars for connection string and JWT secret
- `db` — `mcr.microsoft.com/mssql/server:2022-latest`, volume-mounted data directory
- `hangfire-dashboard` — accessed through the API at `/hangfire`

---

## README.md

Include:
- Project overview and architecture diagram (ASCII)
- Prerequisites (Docker, .NET 9 SDK, SQL Server)
- Local development setup (step by step)
- Running migrations
- Seeding the database
- Running tests
- Environment variable reference table
- API documentation URL (`/index.html` → Swagger UI)
- Role permissions matrix table

---

## QUALITY STANDARDS

- Zero compiler warnings — treat warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- Nullable reference types enabled on all projects (`<Nullable>enable</Nullable>`)
- All async methods use `CancellationToken` and pass it through to EF and IO calls
- No `async void` — always `async Task`
- No `DateTime.Now` — always `IDateTimeService.UtcNow`
- No magic strings — use `const` or `static readonly` for all string literals used in multiple places (claim types, policy names, role names, event names)
- `sealed` on all classes that are not designed for inheritance
- `record` types for DTOs, Commands, Queries, and domain events
- Dispose `IDisposable` resources properly — prefer `using` declarations

---

## DELIVERABLES CHECKLIST

The agent must produce all of the following before considering the task complete:

- [ ] `TaxOmbud.sln` with all 7 projects
- [ ] All Domain entities, enums, value objects, domain events, and exceptions
- [ ] All Application commands, queries, handlers, validators, DTOs, interfaces
- [ ] All Infrastructure EF configurations, migrations, service implementations
- [ ] All 19 API controllers with all 181 endpoints implemented
- [ ] All middleware (exception handling, logging, rate limiting, security headers)
- [ ] `Program.cs` — complete pipeline and DI registration
- [ ] `appsettings.json` and `appsettings.Development.json`
- [ ] Database seeder for roles, permissions, lookups, and default admin
- [ ] 5 Hangfire background jobs
- [ ] Webhook dispatch system with HMAC signing
- [ ] Unit tests with ≥ 80% coverage on Domain + Application
- [ ] Integration tests for core workflows
- [ ] `Dockerfile` (multi-stage)
- [ ] `docker-compose.yml`
- [ ] `README.md` with setup instructions and role matrix

**Do not stop until all items above are complete.**
