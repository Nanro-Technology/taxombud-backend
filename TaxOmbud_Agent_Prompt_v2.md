# Tax Ombud Case Management System — Full Project Generation Prompt v2
> For use with AI coding agents (Antigravity, Cursor, GitHub Copilot Workspace, etc.)
> Version 2 — Reconciled against confirmed OpenAPI spec + extended domain coverage

---

## MISSION

Generate a **production-ready, fully functional** backend API for the **South African Tax Ombud Case Management System**. The system enables taxpayers to lodge complaints against SARS (South African Revenue Service), tracks investigations, manages cases through their full lifecycle, handles internal HR operations, and produces statutory reports.

Build the **complete solution** — every layer, every file, no placeholders, no `// TODO` comments, no stub methods. Every interface must have a concrete implementation. Every service must connect to real infrastructure. **Do not stop until every endpoint in the master list is implemented.**

---

## TECH STACK (NON-NEGOTIABLE)

| Layer | Technology |
|---|---|
| Runtime | .NET 9 (latest stable) |
| API Framework | ASP.NET Core 9 — minimal hosting model |
| ORM | Entity Framework Core 9 |
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
| File Storage | Local disk abstracted behind `IStorageService` — interface must be swappable to Azure Blob Storage with zero controller changes |
| Background Jobs | `Hangfire` with MSSQL persistence |
| Testing | xUnit + Moq + FluentAssertions + `Microsoft.AspNetCore.Mvc.Testing` |
| Containerisation | `Dockerfile` (multi-stage) + `docker-compose.yml` |

---

## SOLUTION STRUCTURE — CLEAN ARCHITECTURE

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

### Dependency Rule (strictly enforced)
```
API  ──►  Application  ──►  Domain
Infrastructure  ──►  Application  ──►  Domain
```
- `Domain` has **zero** project or NuGet references (except `MediatR.Contracts` for domain events)
- `Application` references only `Domain`
- `Infrastructure` references `Application` (never `API`)
- `API` references `Application` and `Infrastructure` (for DI registration only)

---

## LAYER 1 — `TaxOmbud.Domain`

### Folder Structure
```
TaxOmbud.Domain/
├── Common/
│   ├── BaseEntity.cs                  # Id (Guid), CreatedAt, UpdatedAt, ISoftDelete
│   ├── BaseAuditableEntity.cs         # + CreatedBy (UserId), UpdatedBy (UserId)
│   ├── IDomainEvent.cs
│   ├── IHasDomainEvents.cs
│   ├── ISoftDelete.cs                 # IsDeleted, DeletedAt, DeletedBy
│   └── PagedResult.cs                 # Items, TotalCount, Page, PageSize, TotalPages
├── Entities/
│   ├── Identity/
│   │   ├── User.cs                    # FirstName, LastName, Email, PasswordHash, Status, MfaEnabled, FailedLoginCount, LockoutEnd, DepartmentId, JobTitle, EmploymentType, PhoneNumber
│   │   ├── Role.cs                    # Name, Code, Scope, Description, IsSystem
│   │   ├── Permission.cs              # Code, Name, Description, Group
│   │   ├── RolePermission.cs          # RoleId, PermissionId (composite PK)
│   │   ├── UserRole.cs                # UserId, RoleId (composite PK)
│   │   ├── UserPermissionOverride.cs  # UserId, PermissionCode, Mode (Grant/Deny)
│   │   ├── RefreshToken.cs            # Token (hashed), UserId, ExpiresAt, RevokedAt, ReplacedByToken
│   │   └── MfaToken.cs               # UserId, SecretKey, BackupCodes (JSON), IsEnabled
│   ├── Taxpayers/
│   │   ├── Taxpayer.cs                # UserId, TIN, NIN, BVN, Gender, DateOfBirth, TaxpayerType, CompanyName, RcNumber, IsVerified, VerifiedAt, VerifiedBy
│   │   ├── TaxpayerAddress.cs         # TaxpayerId, Line1, City, State, PostalCode, Country
│   │   └── TaxpayerContactDetail.cs   # TaxpayerId, Phone, Email, PreferredChannel
│   ├── Officers/
│   │   ├── Officer.cs                 # UserId, EmployeeNumber, Specialisation, MaxCaseload, IsAvailable, DepartmentId
│   │   └── OfficerPerformanceRecord.cs # OfficerId, Period, CasesResolved, AvgResolutionDays, SlaBreaches
│   ├── Organisation/
│   │   └── Department.cs              # Name, Description, RoutingMode (RoundRobin/Manual/LoadBalanced), HeadUserId
│   ├── Complaints/
│   │   ├── Complaint.cs               # ReferenceNumber, TaxpayerId, AssignedOfficerId, TaxType, TaxPeriod, ComplaintCategory, Subject, Description, TaxOfficeRef, TinNumber, Status, CurrentStage, Priority, RequiresApprovalToClose, ClosedAt, ClosureReason, WithdrawalReason
│   │   ├── ComplaintStatusHistory.cs  # ComplaintId, FromStatus, ToStatus, ChangedBy, Reason, ChangedAt
│   │   ├── ComplaintNote.cs           # ComplaintId, AuthorId, Text, IsExternal
│   │   └── ComplaintLink.cs           # ComplaintId, LinkedComplaintId (composite PK), LinkType
│   ├── Cases/
│   │   ├── Case.cs                    # CaseNumber, ComplaintId, AssignedOfficerId, Status, Stage, Summary, DueDate, ClosedAt, OutcomeType, ClosureNotes
│   │   ├── CaseQueue.cs               # Name, Description, DepartmentId, RoutingRule
│   │   ├── CaseQueueAssignment.cs     # CaseId, QueueName, AssignedAt
│   │   ├── CaseStatusHistory.cs       # CaseId, FromStatus, ToStage, ChangedBy, Reason, ChangedAt
│   │   ├── CaseMilestone.cs           # CaseId, Name, DueDate, CompletedAt, CompletedBy, IsStatutory
│   │   ├── CaseFinding.cs             # CaseId, FindingType, Description, TaxAmountInDispute (decimal 18,2), RecordedBy
│   │   ├── CaseRecommendation.cs      # CaseId, RecommendationText, RecommendedAdjustmentAmount (decimal 18,2), PostedBy, ApprovedBy
│   │   └── CaseNote.cs                # CaseId, AuthorId, Text, IsExternal
│   ├── Documents/
│   │   ├── Document.cs                # FileName, FilePath, ContentType, FileSize, EntityType, EntityId, UploadedBy, IsDeleted
│   │   └── DocumentVersion.cs         # DocumentId, VersionNumber, FilePath, ContentType, FileSize, UploadedBy
│   ├── Communications/
│   │   ├── Communication.cs           # Channel, Subject, Body, Recipient, RecipientName, Direction, RelatedEntityId, RelatedEntityType, SentAt, AcknowledgedAt, LoggedBy
│   │   └── CommunicationTemplate.cs   # Name, Channel, SubjectTemplate, BodyTemplate, Variables (JSON)
│   ├── Appeals/
│   │   ├── Appeal.cs                  # CaseId, FiledBy, Reason, Status, ReviewNotes, ReviewedBy, ReviewedAt, Decision, DecisionNotes
│   │   └── AppealStatusHistory.cs     # AppealId, FromStatus, ToStatus, ChangedBy, ChangedAt
│   ├── Appointments/
│   │   └── Appointment.cs             # Title, Description, StartTime, EndTime, TaxpayerId, OfficerId, Location, MeetingUrl, Status, CompletionNotes
│   ├── Notifications/
│   │   ├── Notification.cs            # UserId, Title, Message, IsRead, ReadAt, EntityType, EntityId
│   │   └── NotificationPreference.cs  # UserId, Channel, EventType, IsEnabled
│   ├── HR/
│   │   ├── StaffProfile.cs            # UserId, HireDate, EmploymentStatus, DateOfBirth, Nationality, MaritalStatus, EmergencyContact, BankAccountNo, BankId, NextOfKin
│   │   ├── LeaveRequest.cs            # UserId, LeaveType, StartDate, EndDate, Status, SupervisorNote, ApprovedBy, ApprovedAt
│   │   ├── EwaWallet.cs               # UserId, AvailableBalance (decimal), TotalEarned (decimal), LastUpdated
│   │   ├── EwaWithdrawal.cs           # UserId, Amount (decimal), RequestedAt, ProcessedAt, Status
│   │   ├── LoanRequest.cs             # UserId, Amount (decimal), TermMonths, Purpose, Status, ApprovedBy, ApprovedAt
│   │   ├── PayGrade.cs                # Name, Level, BasicSalaryBandMin (decimal), BasicSalaryBandMax (decimal)
│   │   ├── SalaryProfile.cs           # UserId, PayGradeId, Basic (decimal), Allowances (JSON), Deductions (JSON), EffectiveFrom
│   │   ├── PayrollPeriod.cs           # Year, Month, StartDate, EndDate, Status (Open/Closed)
│   │   └── PayrollRun.cs              # PeriodId, RunAt, RunBy, Status, TotalGross (decimal), TotalNet (decimal)
│   └── System/
│       ├── AuditLog.cs                # EntityType, EntityId, Action, OldValues (JSON), NewValues (JSON), UserId, IpAddress, UserAgent, CreatedAt — APPEND-ONLY
│       ├── SystemSetting.cs           # Key, Value, Description, IsEncrypted
│       ├── FeatureFlag.cs             # Name, IsEnabled, Description, LastToggledBy
│       ├── WebhookSubscription.cs     # Url, SecretHash, EventTypes (CSV), IsActive, CreatedBy
│       ├── WebhookDelivery.cs         # SubscriptionId, EventType, Payload, StatusCode, AttemptCount, LastAttemptAt, Succeeded
│       └── ScheduledReport.cs         # ReportName, CronExpression, Recipients (JSON), Format, IsActive, LastRunAt
├── Enums/
│   ├── ComplaintStatus.cs             # Draft, Submitted, UnderReview, Escalated, Resolved, Closed, Withdrawn
│   ├── ComplaintStage.cs              # Intake, Assessment, Investigation, Recommendation, Closure
│   ├── ComplaintPriority.cs           # Low, Normal, High, Urgent
│   ├── CaseStatus.cs                  # Open, InProgress, PendingSarsResponse, UnderReview, PendingApproval, Closed
│   ├── CaseStage.cs                   # Intake, Assessment, Investigation, Recommendation, Closure
│   ├── AppealStatus.cs                # Submitted, UnderReview, Upheld, Dismissed
│   ├── AppointmentStatus.cs           # Scheduled, Confirmed, Completed, Cancelled
│   ├── DocumentEntityType.cs          # Complaint, Case, Appeal, Communication
│   ├── CommunicationDirection.cs      # Inbound, Outbound
│   ├── CommunicationChannel.cs        # Email, Letter, Phone, Portal, InPerson
│   ├── UserStatus.cs                  # Active, Inactive, Locked
│   ├── TaxpayerType.cs                # Individual, Company, Trust, Partnership
│   ├── LeaveStatus.cs                 # Pending, Approved, Rejected, Cancelled
│   ├── LeaveType.cs                   # Annual, Sick, Maternity, Paternity, Unpaid, Study
│   ├── LoanStatus.cs                  # Pending, Approved, Rejected, Disbursed, Repaid
│   ├── PayrollStatus.cs               # Draft, Processing, Completed, Failed
│   ├── DepartmentRoutingMode.cs       # RoundRobin, Manual, LoadBalanced
│   └── PermissionOverrideMode.cs      # Grant, Deny
├── Events/
│   ├── Complaints/
│   │   ├── ComplaintSubmittedEvent.cs
│   │   ├── ComplaintStatusChangedEvent.cs
│   │   └── ComplaintEscalatedEvent.cs
│   ├── Cases/
│   │   ├── CaseOpenedEvent.cs
│   │   ├── CaseAssignedEvent.cs
│   │   ├── CaseTransitionedEvent.cs
│   │   └── CaseClosedEvent.cs
│   ├── Appeals/
│   │   ├── AppealSubmittedEvent.cs
│   │   ├── AppealUpheldEvent.cs
│   │   └── AppealDismissedEvent.cs
│   ├── Taxpayers/
│   │   ├── TaxpayerRegisteredEvent.cs
│   │   └── TaxpayerVerifiedEvent.cs
│   └── HR/
│       ├── LeaveApprovedEvent.cs
│       └── LoanApprovedEvent.cs
├── Exceptions/
│   ├── DomainException.cs
│   ├── NotFoundException.cs
│   ├── ForbiddenException.cs
│   ├── ConflictException.cs
│   └── ValidationException.cs
└── ValueObjects/
    ├── TaxIdentificationNumber.cs     # Validated SA/NG TIN format, implicit string conversion
    ├── ReferenceNumber.cs             # TO-{YYYY}-{000001}, parse + generate
    ├── CaseNumber.cs                  # CASE-{YYYY}-{000001}
    └── Email.cs                       # Normalised, validated email value object
```

### Entity Business Rule Requirements
- All entities inherit `BaseAuditableEntity`
- State-machine entities (`Complaint`, `Case`, `Appeal`, `LeaveRequest`, `LoanRequest`) must encapsulate all transitions as **domain methods** — no external `Status = X` mutations. Example:

```csharp
// Case.cs
public Result Transition(string targetStage, string reason, Guid userId)
{
    var allowed = AllowedTransitions[Stage];
    if (!allowed.Contains(targetStage))
        return Result.Failure($"Cannot transition from {Stage} to {targetStage}.");
    Stage = targetStage;
    AddDomainEvent(new CaseTransitionedEvent(Id, Stage, targetStage, reason, userId));
    return Result.Success();
}
```

- `Complaint.ReferenceNumber` → `TO-{YYYY}-{6-digit-zero-padded-sequence}` — DB-sequenced, unique
- `Case.CaseNumber` → `CASE-{YYYY}-{6-digit-zero-padded-sequence}` — DB-sequenced, unique
- All `decimal` financial fields: `precision(18,2)`
- `AuditLog` is append-only — never expose update/delete from domain or application

---

## LAYER 2 — `TaxOmbud.Application`

References: `TaxOmbud.Domain`, `MediatR`, `FluentValidation`, `Mapster`

### Folder Structure
```
TaxOmbud.Application/
├── Common/
│   ├── Behaviours/
│   │   ├── ValidationBehaviour.cs         # Pipeline: runs all FluentValidation validators; returns Result.Failure on error
│   │   ├── LoggingBehaviour.cs            # Pipeline: structured log per request with duration
│   │   ├── AuthorizationBehaviour.cs      # Pipeline: checks [Authorize(Permission="...")] on commands
│   │   └── AuditBehaviour.cs             # Pipeline: creates AuditLog for every state-changing command
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs       # All DbSet<T> + SaveChangesAsync
│   │   ├── ICurrentUserService.cs         # UserId, Email, Roles, Permissions, IsAuthenticated, IsImpersonating, OriginalUserId
│   │   ├── IDateTimeService.cs            # UtcNow — never use DateTime.Now directly
│   │   ├── IStorageService.cs             # UploadAsync, DownloadAsync, DeleteAsync, GetDownloadUrlAsync
│   │   ├── IEmailService.cs               # SendAsync(EmailMessage)
│   │   ├── ITokenService.cs               # GenerateAccessToken, GenerateRefreshToken, ValidateRefreshToken
│   │   ├── IMfaService.cs                 # GenerateSecret, ValidateTotp, GenerateBackupCodes, ValidateBackupCode
│   │   ├── IAuditService.cs               # LogAsync(AuditEntry)
│   │   ├── IReferenceNumberService.cs     # GenerateComplaintRef, GenerateCaseNumber
│   │   ├── INinVerificationService.cs     # VerifyNin(string nin) — external identity check
│   │   └── IImpersonationService.cs       # StartImpersonation, StopImpersonation
│   ├── Models/
│   │   ├── Result.cs                      # Result<T>: IsSuccess, IsNotFound, IsForbidden, IsConflict, Errors, Value
│   │   ├── PagedResult.cs
│   │   └── EmailMessage.cs                # To, Subject, HtmlBody, Attachments
│   └── Mappings/
│       └── MappingConfig.cs               # Mapster TypeAdapterConfig global setup
├── Features/
│   ├── Auth/
│   │   └── Commands/
│   │       ├── Register/                  # RegisterCommand, Handler, Validator, RegisterResponse
│   │       ├── Login/                     # LoginCommand (email, password, totpCode), Handler, Validator, LoginResponse
│   │       ├── RefreshToken/
│   │       ├── Logout/
│   │       ├── ForgotPassword/
│   │       ├── ResetPassword/
│   │       ├── ChangePassword/
│   │       ├── VerifyEmail/
│   │       ├── SetupMfa/
│   │       ├── VerifyMfa/
│   │       └── DisableMfa/
│   ├── Complaints/
│   │   ├── Commands/
│   │   │   ├── SubmitComplaint/
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
│   │       ├── GetComplaints/             # Filters: page, pageSize, status, taxType, taxpayerId, officerId, search
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
│   │   │   ├── TransitionCase/            # targetStage + reason
│   │   │   ├── AddCaseNote/
│   │   │   ├── PostCaseRecommendation/
│   │   │   ├── ApproveCaseClosure/        # approve: bool, rationale
│   │   │   ├── AddCaseFinding/
│   │   │   ├── UpdateCaseFinding/
│   │   │   ├── LogCaseCommunication/
│   │   │   ├── UploadCaseDocument/
│   │   │   └── CompleteCaseMilestone/
│   │   └── Queries/
│   │       ├── GetCases/                  # search, stage, status, page, pageSize
│   │       ├── GetCaseById/
│   │       ├── GetCasesByQueue/           # queueName, page, pageSize
│   │       ├── GetCaseFindings/
│   │       ├── GetCaseRecommendations/
│   │       ├── GetCaseCommunications/
│   │       ├── GetCaseDocuments/
│   │       ├── GetCaseMilestones/
│   │       ├── GetMyCases/
│   │       └── GetOverdueCases/
│   ├── Taxpayers/
│   │   ├── Commands/
│   │   │   ├── UpdateTaxpayer/
│   │   │   ├── VerifyTaxpayer/
│   │   │   └── VerifyNin/
│   │   └── Queries/
│   │       ├── GetTaxpayers/              # search, type, isVerified, page, pageSize
│   │       ├── GetTaxpayerById/
│   │       └── GetTaxpayerComplaints/
│   ├── Officers/
│   │   ├── Commands/
│   │   │   ├── CreateOfficerProfile/
│   │   │   └── UpdateOfficerProfile/
│   │   └── Queries/
│   │       ├── GetOfficers/               # departmentId, search, page, pageSize
│   │       ├── GetOfficerById/
│   │       └── GetOfficerCaseloads/       # activeOnly filter
│   ├── Departments/
│   │   ├── Commands/
│   │   │   ├── CreateDepartment/
│   │   │   └── UpdateDepartment/
│   │   └── Queries/
│   │       ├── GetDepartments/
│   │       └── GetDepartmentById/
│   ├── Documents/
│   │   ├── Commands/
│   │   │   ├── CreateDocument/
│   │   │   ├── DeleteDocument/
│   │   │   ├── AddDocumentVersion/
│   │   │   └── ClassifyDocument/
│   │   └── Queries/
│   │       ├── GetDocuments/              # entityId, entityType, page, pageSize
│   │       ├── GetDocumentById/
│   │       └── GetDocumentDownloadUrl/
│   ├── Communications/
│   │   ├── Commands/
│   │   │   ├── LogCommunication/
│   │   │   └── SendCommunication/
│   │   └── Queries/
│   │       ├── GetCommunications/         # relatedEntityId, relatedEntityType, channel, direction
│   │       └── GetCommunicationById/
│   ├── Appeals/
│   │   ├── Commands/
│   │   │   ├── FileAppeal/
│   │   │   ├── ReviewAppeal/              # action (Uphold/Dismiss), notes
│   │   │   └── UploadAppealDocument/
│   │   └── Queries/
│   │       ├── GetAppeals/
│   │       ├── GetAppealById/
│   │       └── GetAppealDocuments/
│   ├── Appointments/
│   │   ├── Commands/
│   │   │   ├── BookAppointment/
│   │   │   └── UpdateAppointmentStatus/
│   │   └── Queries/
│   │       ├── GetAppointments/           # taxpayerId, officerId, status
│   │       └── GetAppointmentById/
│   ├── Notifications/
│   │   ├── Commands/
│   │   │   ├── SendNotification/
│   │   │   ├── MarkNotificationRead/
│   │   │   ├── MarkAllNotificationsRead/
│   │   │   └── DeleteNotification/
│   │   └── Queries/
│   │       └── GetNotifications/          # unreadOnly, page, pageSize
│   ├── Reports/
│   │   └── Queries/
│   │       ├── GetDashboard/
│   │       ├── GetComplaintsByTaxType/
│   │       ├── GetComplaintsByStatus/
│   │       ├── GetComplaintsByStage/
│   │       ├── GetComplaintsMonthlyTrend/ # year filter
│   │       ├── GetOfficersWorkload/
│   │       ├── GetScheduledReports/
│   │       ├── CreateScheduledReport/
│   │       ├── ToggleScheduledReport/
│   │       └── DeleteScheduledReport/
│   ├── Users/
│   │   ├── Commands/
│   │   │   ├── CreateUser/
│   │   │   ├── UpdateUser/
│   │   │   ├── UpdateUserStatus/          # activate: bool
│   │   │   ├── AssignRoles/
│   │   │   └── SetPermissionOverrides/
│   │   └── Queries/
│   │       ├── GetUsers/                  # search, status, departmentId, page, pageSize
│   │       └── GetUserById/
│   ├── Roles/
│   │   ├── Commands/
│   │   │   ├── CreateRole/
│   │   │   └── UpdateRolePermissions/
│   │   └── Queries/
│   │       ├── GetRoles/
│   │       ├── GetRoleById/
│   │       └── GetPermissions/
│   ├── AuditLogs/
│   │   └── Queries/
│   │       ├── GetAuditLogs/              # entityType, entityId, userId, action, from, to, page, pageSize
│   │       └── GetAuditLogById/
│   ├── System/
│   │   ├── Commands/
│   │   │   ├── UpdateSetting/
│   │   │   ├── ToggleFeatureFlag/
│   │   │   ├── StartImpersonation/
│   │   │   └── StopImpersonation/
│   │   └── Queries/
│   │       ├── GetSettings/
│   │       ├── GetSystemAuditLogs/
│   │       └── GetFeatureFlags/
│   ├── Webhooks/
│   │   ├── Commands/
│   │   │   ├── CreateWebhook/
│   │   │   ├── UpdateWebhook/
│   │   │   ├── DeleteWebhook/
│   │   │   └── RotateWebhookSecret/
│   │   └── Queries/
│   │       ├── GetWebhooks/
│   │       └── GetWebhookById/
│   └── HR/
│       ├── Commands/
│       │   ├── SaveStaffProfile/
│       │   ├── RequestLeave/
│       │   ├── ApproveLeave/
│       │   ├── EwaWithdrawal/
│       │   ├── RequestLoan/
│       │   ├── ApproveLoan/
│       │   ├── CreatePayrollRun/
│       │   ├── CreatePayGrade/
│       │   ├── UpdatePayGrade/
│       │   ├── DeletePayGrade/
│       │   └── SaveSalaryProfile/
│       └── Queries/
│           ├── GetStaff/
│           ├── GetStaffById/
│           ├── GetLeaveRequests/
│           ├── GetWallet/
│           ├── GetPayrollPeriods/
│           ├── GetPayGrades/
│           ├── GetPayGradeById/
│           └── GetSalaryProfiles/
└── DependencyInjection.cs              # AddApplication(IServiceCollection) extension method
```

### Handler Rules
- Every Command/Query is `IRequest<Result<T>>`
- Every handler implements `IRequestHandler<TRequest, Result<T>>`
- Handlers NEVER access `HttpContext` — use `ICurrentUserService`
- All reads use `.AsNoTracking()`
- After `SaveChangesAsync`, dispatch domain events via `IPublisher.Publish()`
- `ValidationBehaviour` runs before every handler — rejects with 422 on invalid input

---

## LAYER 3 — `TaxOmbud.Infrastructure`

```
TaxOmbud.Infrastructure/
├── Persistence/
│   ├── TaxOmbudDbContext.cs
│   ├── Configurations/                # One IEntityTypeConfiguration<T> per entity (55+ files)
│   ├── Migrations/
│   └── Seeders/
│       ├── RoleSeeder.cs
│       ├── PermissionSeeder.cs
│       ├── DepartmentSeeder.cs
│       ├── LookupSeeder.cs
│       └── AdminUserSeeder.cs
├── Identity/
│   ├── TokenService.cs
│   ├── PasswordService.cs
│   └── MfaService.cs
├── Services/
│   ├── CurrentUserService.cs
│   ├── DateTimeService.cs
│   ├── EmailService.cs
│   ├── StorageService.cs
│   ├── ReferenceNumberService.cs
│   ├── AuditService.cs
│   ├── NinVerificationService.cs      # Stub — calls configurable external endpoint
│   ├── ImpersonationService.cs
│   └── WebhookDispatchService.cs
├── BackgroundJobs/
│   ├── HangfireJobScheduler.cs
│   ├── SlaBreachNotificationJob.cs
│   ├── SendScheduledReportJob.cs
│   ├── WebhookRetryJob.cs
│   ├── CleanupExpiredTokensJob.cs
│   └── NotificationCleanupJob.cs
└── DependencyInjection.cs
```

### EF Core Configuration Rules
- Every entity has its own `IEntityTypeConfiguration<T>`
- All `string` columns: explicit `HasMaxLength()`
- All `enum` columns: stored as `int`
- All `decimal` columns: `HasPrecision(18, 2)`
- All `DateTime` columns: `datetime2(7)` — never `datetime`
- Global soft-delete query filters on all `ISoftDelete` entities
- `ValueConverter` for `TaxIdentificationNumber`, `ReferenceNumber`, `CaseNumber`, `Email` value objects
- `newsequentialid()` as default for all GUID PKs (performance on clustered index)
- **Required indexes:**
  - `Complaints`: unique(`ReferenceNumber`), composite(`Status`, `CreatedAt`), `TaxpayerId`
  - `Cases`: unique(`CaseNumber`), composite(`Status`, `AssignedOfficerId`)
  - `Taxpayers`: unique(`TaxIdentificationNumber`), `Email`
  - `Users`: unique(`Email`)
  - `AuditLogs`: composite(`EntityType`, `EntityId`), `CreatedAt`
  - `RefreshTokens`: unique(`Token`)
  - `Notifications`: composite(`UserId`, `IsRead`)
  - `Documents`: composite(`EntityType`, `EntityId`)
  - `LeaveRequests`: composite(`UserId`, `Status`)

---

## LAYER 4 — `TaxOmbud.API`

```
TaxOmbud.API/
├── Controllers/v1/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── TaxpayersController.cs
│   ├── OfficersController.cs
│   ├── DepartmentsController.cs
│   ├── ComplaintsController.cs
│   ├── CasesController.cs
│   ├── DocumentsController.cs
│   ├── CommunicationsController.cs
│   ├── AppealsController.cs
│   ├── AppointmentsController.cs
│   ├── NotificationsController.cs
│   ├── ReportsController.cs
│   ├── RolesController.cs
│   ├── AuditLogsController.cs
│   ├── SystemController.cs
│   ├── WebhooksController.cs
│   ├── HrController.cs
│   ├── PayGradesController.cs
│   └── HealthController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs  # RFC 7807 ProblemDetails for all unhandled exceptions
│   ├── RequestLoggingMiddleware.cs
│   ├── RateLimitingMiddleware.cs
│   └── SecurityHeadersMiddleware.cs
├── Filters/
│   └── ApiKeyAuthFilter.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   ├── WebApplicationExtensions.cs
│   └── ClaimsPrincipalExtensions.cs
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### Controller Base Pattern
```csharp
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender _mediator = null!;
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected ActionResult<T> HandleResult<T>(Result<T> result) => result switch
    {
        { IsSuccess: true, Value: not null } => Ok(result.Value),
        { IsSuccess: true }                  => NoContent(),
        { IsNotFound: true }                 => NotFound(new ProblemDetails { Detail = result.Errors.First() }),
        { IsForbidden: true }                => Forbid(),
        { IsConflict: true }                 => Conflict(new ProblemDetails { Detail = result.Errors.First() }),
        { IsUnprocessable: true }            => UnprocessableEntity(new ValidationProblemDetails()),
        _                                    => BadRequest(new ProblemDetails { Detail = string.Join("; ", result.Errors) })
    };
}
```

**ALL routes use the prefix `api/v1/` — no mixed casing, no exceptions.**

---

## COMPLETE ENDPOINT MASTER LIST — 207 ENDPOINTS

Implement every endpoint below. For each: Controller action + MediatR Command or Query + Handler + Validator (commands only) + DTO/Response types.

---

### AUTH — 11 endpoints  `[Route("api/v1/auth")]`

| Method | Route | Access | Request Schema | Response | Notes |
|--------|-------|--------|---------------|----------|-------|
| POST | `/register` | Public | `RegisterCommand` { firstName, lastName, email, password, phoneNumber } | 201 `RegisterResponse` { userId, email, fullName } / 409 / 422 | Sends verification email; assigns Taxpayer role |
| POST | `/login` | Public | `LoginCommand` { email, password, totpCode? } | 200 `LoginResponse` { accessToken, refreshToken, expiresAt, mfaRequired, userId, fullName, roles } / 400 | Lockout after 5 failures; if MFA enabled and totpCode absent, return mfaRequired=true |
| POST | `/refresh` | Public | `RefreshTokenCommand` { token } | 200 `RefreshTokenResponse` { accessToken, newRefreshToken, expiresAt } / 400 | Rotate refresh token; invalidate old |
| POST | `/logout` | Authenticated | `{ refreshToken }` | 204 | Revoke refresh token in DB |
| POST | `/forgot-password` | Public | `{ email }` | 204 | Always 204 — no enumeration of emails |
| POST | `/reset-password` | Public | `{ token, newPassword, confirmPassword }` | 204 / 400 | Token valid 1 hour |
| POST | `/change-password` | Authenticated | `{ currentPassword, newPassword, confirmPassword }` | 204 / 400 | |
| POST | `/verify-email` | Public | `{ token }` | 204 / 400 | Token valid 24 hours |
| POST | `/mfa/setup` | Authenticated | — | 200 `{ qrUri, secret, backupCodes[] }` | Generates TOTP secret; does NOT enable MFA yet |
| POST | `/mfa/verify` | Authenticated | `{ code }` | 204 | Validates TOTP; enables MFA on account |
| POST | `/mfa/disable` | Authenticated | `{ code }` | 204 | Requires valid TOTP to disable |

---

### USERS — 10 endpoints  `[Route("api/v1/users")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Admin | `?search&status&departmentId&page&pageSize` | Paginated |
| POST | `/` | Admin | `CreateUserRequest` { firstName, lastName, email, password, phone, jobTitle, employmentType, departmentId? } | 201 / 400 — sends welcome email |
| GET | `/{id}` | Admin | — | 200 / 404 |
| PUT | `/{id}` | Admin | `UpdateUserRequest` { firstName, lastName, phone, jobTitle, employmentType, departmentId? } | 204 / 404 |
| PUT | `/{id}/status` | Admin | `UpdateUserStatusRequest` { activate: bool }` | 204 / 404 — cannot deactivate own account |
| POST | `/{id}/roles` | Admin | `AssignRolesRequest` { roleIds[] }` | 204 / 400 |
| POST | `/{id}/permissions/overrides` | Admin | `PermissionOverridesRequest` { overrides[]: { permissionCode, mode } }` | 204 / 400 |
| GET | `/me` | Authenticated | — | 200 — own profile |
| PUT | `/me` | Authenticated | `UpdateProfileRequest` | 200 |
| GET | `/{id}/audit-log` | Admin | `?page&pageSize` | 200 — paginated activity log |

---

### TAXPAYERS — 8 endpoints  `[Route("api/v1/taxpayers")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Officer,Supervisor,Admin | `?search&type&isVerified&page&pageSize` | |
| GET | `/{id}` | Authenticated | — | Taxpayer sees own only |
| PUT | `/{id}` | Authenticated | `UpdateTaxpayerRequest` { firstName, lastName, phone, tinNumber, nin, bvn, gender, dateOfBirth, companyName, rcNumber, address, city, state } | 204 / 404 |
| POST | `/{id}/verify` | Officer,Admin | `VerifyTaxpayerRequest` { isVerified: bool }` | 204 / 404 |
| POST | `/verify-nin` | Officer,Admin | `NinVerificationRequest` { nin }` | 200 `{ isValid, name?, dateOfBirth? }` / 400 |
| GET | `/{id}/complaints` | Officer,Supervisor,Admin | `?page&pageSize` | All complaints filed by this taxpayer |
| GET | `/me` | Taxpayer | — | Own profile |
| PATCH | `/{id}/deactivate` | Admin | — | 204 |

---

### OFFICERS — 6 endpoints  `[Route("api/v1/officers")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Supervisor,Admin | `?departmentId&search&page&pageSize` | Includes active case count |
| POST | `/` | Admin | `CreateOfficerProfileRequest` { userId, maxCaseload, employeeNumber, specialisation }` | 201 / 400 |
| GET | `/{id}` | Supervisor,Admin | — | 200 / 404 |
| PUT | `/{id}` | Supervisor,Admin | `UpdateOfficerProfileRequest` { maxCaseload, isAvailable, employeeNumber, specialisation }` | 204 / 404 |
| GET | `/{id}/caseloads` | Supervisor,Admin | `?activeOnly` | 200 / 404 |
| GET | `/available` | Supervisor,Admin | — | Officers with remaining capacity |

---

### DEPARTMENTS — 4 endpoints  `[Route("api/v1/departments")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Authenticated | — | All departments |
| POST | `/` | Admin | `CreateDepartmentRequest` { name, routingMode, description, headUserId? }` | 201 / 400 |
| GET | `/{id}` | Authenticated | — | 200 / 404 |
| PUT | `/{id}` | Admin | `UpdateDepartmentRequest` { name, routingMode, description, headUserId? }` | 204 / 400 / 404 |

---

### COMPLAINTS — 18 endpoints  `[Route("api/v1/complaints")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Officer,Supervisor,Admin | `?page&pageSize&status&taxType&taxpayerId&officerId&search` | |
| POST | `/` | Authenticated | `SubmitComplaintCommand` { taxpayerId, taxType, taxPeriod, complaintCategory, subject, description, taxOfficeRef, tinNumber }` | 201 `SubmitComplaintResponse` { complaintId, referenceNumber, status } / 422 |
| GET | `/{id}` | Authenticated | — | `ComplaintDetailDto` / 404; taxpayer sees own only |
| GET | `/reference/{refNo}` | Public | — | Status + summary only |
| PUT | `/{id}` | Authenticated | `UpdateComplaintRequest` | 204 — only Draft status |
| DELETE | `/{id}` | Supervisor,Admin | — | 204 soft-delete |
| PATCH | `/{id}/status` | Officer,Supervisor,Admin | `{ status, reason }` | 204 — validates allowed transition |
| POST | `/{id}/assign` | Supervisor,Admin | `{ officerId }` | 204 |
| PATCH | `/{id}/escalate` | Officer,Supervisor,Admin | `{ reason }` | 204 |
| PATCH | `/{id}/close` | Supervisor,Admin | `{ closureReason }` | 204 — requires reason |
| PATCH | `/{id}/reopen` | Supervisor,Admin | `{ reason }` | 204 |
| GET | `/{id}/timeline` | Authenticated | — | Status history |
| GET | `/{id}/notes` | Officer,Supervisor,Admin | — | Internal notes |
| POST | `/{id}/notes` | Officer,Supervisor,Admin | `AddNoteRequest` { text, isExternal }` | 200 |
| GET | `/{id}/documents` | Authenticated | — | |
| POST | `/{id}/documents` | Authenticated | `CreateDocumentRequest` multipart | 201 |
| GET | `/{id}/related` | Officer,Supervisor,Admin | — | Linked complaints |
| POST | `/{id}/link` | Officer,Supervisor,Admin | `{ linkedComplaintId, linkType }` | 204 |

---

### CASES — 17 endpoints  `[Route("api/v1/cases")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Officer,Supervisor,Admin | `?search&stage&status&page&pageSize` | |
| GET | `/{id}` | Officer,Supervisor,Admin | — | Full detail / 404 |
| GET | `/queues/{queueName}` | Officer,Supervisor,Admin | `?page&pageSize` | Cases in named queue |
| POST | `/{id}/notes` | Officer,Supervisor,Admin | `AddNoteRequest` { text, isExternal }` | 200 / 404 |
| POST | `/{id}/assign` | Supervisor,Admin | `AssignCaseRequest` { officerId }` | 204 / 404 |
| POST | `/{id}/transition` | Officer,Supervisor,Admin | `TransitionCaseRequest` { targetStage, reason }` | 204 / 404 |
| POST | `/{id}/recommendation` | Supervisor,Admin | `PostRecommendationRequest` { recommendationText }` | 200 / 404 |
| POST | `/{id}/approvals` | Supervisor,Admin | `ApproveClosureRequest` { approve: bool, rationale }` | 204 / 400 / 404 |
| GET | `/{id}/findings` | Officer,Supervisor,Admin | — | |
| POST | `/{id}/findings` | Officer,Supervisor,Admin | `AddCaseFindingRequest` { findingType, description, taxAmountInDispute? }` | 201 |
| PUT | `/{id}/findings/{findingId}` | Officer,Supervisor,Admin | `UpdateFindingRequest` | 200 |
| GET | `/{id}/communications` | Officer,Supervisor,Admin | — | SARS correspondence |
| GET | `/{id}/documents` | Officer,Supervisor,Admin | — | |
| POST | `/{id}/documents` | Officer,Supervisor,Admin | `CreateDocumentRequest` | 201 |
| GET | `/{id}/milestones` | Officer,Supervisor,Admin | — | Statutory milestones |
| PATCH | `/{id}/milestones/{milestoneId}` | Officer,Supervisor,Admin | `CompleteMilestoneRequest` | 200 |
| GET | `/my` | Officer | — | My assigned cases |

---

### DOCUMENTS — 6 endpoints  `[Route("api/v1/documents")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Officer,Supervisor,Admin | `?entityId&entityType&page&pageSize` | |
| POST | `/` | Authenticated | `CreateDocumentRequest` { fileName, filePath, contentType, fileSize, entityType, entityId }` | 201 / 400 |
| GET | `/{id}` | Authenticated | — | Metadata / 404 |
| DELETE | `/{id}` | Officer,Supervisor,Admin | — | 204 soft-delete / 404 |
| POST | `/{id}/versions` | Authenticated | `AddDocumentVersionRequest` { filePath }` | 201 / 404 |
| GET | `/{id}/download-url` | Authenticated | — | 200 `{ url, expiresAt }` / 404 — pre-signed or local URL |

---

### COMMUNICATIONS — 3 endpoints  `[Route("api/v1/communications")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Officer,Supervisor,Admin | `?relatedEntityId&relatedEntityType&channel&direction&page&pageSize` | |
| POST | `/` | Officer,Supervisor,Admin | `LogCommunicationRequest` { channel, subject, body, recipient, recipientName, relatedEntityId?, relatedEntityType? }` | 201 / 400 |
| GET | `/{id}` | Officer,Supervisor,Admin | — | 200 / 404 |

---

### APPEALS — 4 endpoints  `[Route("api/v1/appeals")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Officer,Supervisor,Admin | `?status&page&pageSize` | |
| POST | `/` | Authenticated | `FileAppealRequest` { caseId, reason }` | 201 / 400 |
| GET | `/{id}` | Authenticated | — | 200 / 404; taxpayer sees own only |
| POST | `/{id}/review` | Supervisor,Admin | `ReviewAppealRequest` { action (Uphold/Dismiss), notes }` | 204 / 400 / 404 |

---

### APPOINTMENTS — 4 endpoints  `[Route("api/v1/appointments")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Authenticated | `?taxpayerId&officerId&status` | Role-filtered |
| POST | `/` | Officer,Supervisor,Admin | `BookAppointmentRequest` { title, description, startTime, endTime, taxpayerId?, officerId?, location?, meetingUrl? }` | 201 / 400 |
| GET | `/{id}` | Authenticated | — | 200 / 404 |
| PUT | `/{id}/status` | Authenticated | `UpdateAppointmentStatusRequest` { status }` | 204 / 400 / 404 |

---

### NOTIFICATIONS — 5 endpoints  `[Route("api/v1/notifications")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Authenticated | `?unreadOnly&page&pageSize` | Own notifications only |
| POST | `/` | Officer,Supervisor,Admin | `SendNotificationRequest` { userId, title, message }` | 201 |
| PUT | `/{id}/read` | Authenticated | — | 204 / 404 |
| PUT | `/read-all` | Authenticated | — | 204 |
| DELETE | `/{id}` | Authenticated | — | 204 / 404 |

---

### REPORTS — 10 endpoints  `[Route("api/v1/reports")]`

| Method | Route | Roles | Notes |
|--------|-------|-------|-------|
| GET | `/dashboard` | Officer,Supervisor,Admin | KPIs: total complaints, open cases, SLA breaches, avg resolution days |
| GET | `/complaints/by-tax-type` | Officer,Supervisor,Admin | Grouped count by TaxType |
| GET | `/complaints/by-status` | Officer,Supervisor,Admin | Grouped count by Status |
| GET | `/complaints/by-stage` | Officer,Supervisor,Admin | Grouped count by Stage |
| GET | `/complaints/monthly-trend` | Officer,Supervisor,Admin | `?year` — monthly complaint volumes |
| GET | `/officers/workload` | Supervisor,Admin | Per-officer active case count, SLA breach count |
| GET | `/scheduled` | Admin | List scheduled report jobs |
| POST | `/scheduled` | Admin | `CreateScheduledReportRequest` { reportName, cronExpression, recipients[], format }` — 201 |
| PUT | `/scheduled/{id}/toggle` | Admin | Enable/disable — 204 / 404 |
| DELETE | `/scheduled/{id}` | Admin | 204 / 404 |

---

### ROLES — 5 endpoints  `[Route("api/v1/roles")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Admin | — | All roles |
| POST | `/` | Admin | `CreateRoleRequest` { name, code, scope, description }` | 201 / 400 |
| GET | `/{id}` | Admin | — | Role + its permissions / 404 |
| PUT | `/{id}/permissions` | Admin | `UpdateRolePermissionsRequest` { permissionCodes[] }` | 204 / 400 |
| GET | `/permissions` | Admin | — | All available system permissions |

---

### AUDIT LOGS — 2 endpoints  `[Route("api/v1/audit-logs")]`

| Method | Route | Roles | Notes |
|--------|-------|-------|-------|
| GET | `/` | Supervisor,Admin | `?entityType&entityId&userId&action&from&to&page&pageSize` |
| GET | `/{id}` | Supervisor,Admin | 200 / 404 |

---

### SYSTEM — 7 endpoints  `[Route("api/v1/system")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/settings` | Admin | — | All system settings |
| PUT | `/settings` | Admin | `UpdateSettingRequest` { key, value, description }` | 204 |
| GET | `/feature-flags` | Admin | — | All feature flags |
| PUT | `/feature-flags/{id}/toggle` | Admin | — | 204 / 404 |
| GET | `/audit-logs` | Admin | `?entityName&page&pageSize` | System-scope audit logs |
| POST | `/impersonate/{userId}` | Admin | — | 200 `{ accessToken }` / 400 / 403 — issues impersonation JWT; original userId stored in claim |
| POST | `/impersonate/stop` | Admin | — | 200 `{ accessToken }` — returns token for original admin user |

---

### WEBHOOKS — 6 endpoints  `[Route("api/v1/webhooks")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Admin | — | All subscriptions |
| POST | `/` | Admin | `CreateWebhookRequest` { url, secret, eventTypes[] }` | 201 / 400 |
| GET | `/{id}` | Admin | — | 200 / 404 |
| PUT | `/{id}` | Admin | `UpdateWebhookRequest` { url, eventTypes[], isActive }` | 204 / 404 |
| DELETE | `/{id}` | Admin | — | 204 / 404 |
| POST | `/{id}/rotate-secret` | Admin | `RotateSecretRequest` { newSecret }` | 200 `{ newSecret }` / 404 |

---

### HR — 12 endpoints  `[Route("api/v1/hr")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/staff` | Supervisor,Admin | `?search&page&pageSize` | |
| POST | `/staff` | Admin | `SaveStaffProfileRequest` { userId, hireDate, employmentStatus, dateOfBirth, nationality, maritalStatus, emergencyContact, bankAccountNo, bankId, nextOfKin }` | 200 / 400 |
| GET | `/staff/{id}` | Supervisor,Admin | — | 200 / 404 |
| GET | `/leave` | Authenticated | `?userId&status` | Own leave unless Supervisor/Admin |
| POST | `/leave` | Authenticated | `RequestLeaveRequest` { leaveType, startDate, endDate }` | 200 |
| PUT | `/leave/{id}/approve` | Supervisor,Admin | `ApproveLeaveRequest` { approved: bool, supervisorNote? }` | 204 / 404 |
| GET | `/wallet` | Authenticated | — | Own EWA wallet balance |
| POST | `/wallet/withdraw` | Authenticated | `EwaWithdrawalRequest` { amount }` | 200 / 400 — cannot exceed available balance |
| POST | `/loans` | Authenticated | `RequestLoanRequest` { amount, termMonths, purpose }` | 200 |
| PUT | `/loans/{id}/approve` | Supervisor,Admin | `ApproveLoanRequest` { approved: bool }` | 204 / 404 |
| GET | `/payroll/periods` | Admin | — | All payroll periods |
| POST | `/payroll/runs` | Admin | `CreatePayrollRunRequest` { periodId }` | 200 / 400 |

---

### PAY GRADES — 7 endpoints  `[Route("api/v1/hr/pay-grades")]`

| Method | Route | Roles | Request Schema | Notes |
|--------|-------|-------|---------------|-------|
| GET | `/` | Admin | — | All pay grades |
| POST | `/` | Admin | `CreatePayGradeRequest` { name, level, basicSalaryBand }` | 201 / 400 |
| GET | `/{id}` | Admin | — | 200 / 404 |
| PUT | `/{id}` | Admin | `UpdatePayGradeRequest` { name, level, basicSalaryBand }` | 204 / 404 |
| DELETE | `/{id}` | Admin | — | 204 / 404 |
| GET | `/salary-profiles` | Admin | `?userId` | All or per-user |
| POST | `/salary-profiles` | Admin | `SaveSalaryProfileRequest` { userId, basic, allowances, deductions, effectiveFrom }` | 200 / 400 |

---

### HEALTH — 4 endpoints  `[Route("api/v1/health")]`

| Method | Route | Access | Notes |
|--------|-------|--------|-------|
| GET | `/` | Public | Liveness probe — 200 OK or 503 |
| GET | `/detailed` | Admin | DB, SMTP, storage dependency checks |
| GET | `/ready` | Public | Kubernetes readiness probe |
| GET | `/version` | Public | `{ version, environment, commitSha }` |

---

### ENDPOINT SUMMARY TABLE

| Controller | Count |
|---|---|
| Auth | 11 |
| Users | 10 |
| Taxpayers | 8 |
| Officers | 6 |
| Departments | 4 |
| Complaints | 18 |
| Cases | 17 |
| Documents | 6 |
| Communications | 3 |
| Appeals | 4 |
| Appointments | 4 |
| Notifications | 5 |
| Reports | 10 |
| Roles | 5 |
| Audit Logs | 2 |
| System | 7 |
| Webhooks | 6 |
| HR | 12 |
| Pay Grades | 7 |
| Health | 4 |
| **TOTAL** | **149** |

> Note: An additional ~58 sub-resource endpoints for complaints timeline/notes/documents, cases findings/milestones/communications/documents, appeals documents, appointments availability/calendar, search, and lookups are specified above and must also be implemented, bringing the functional total to **≥ 207 routes**.

---

## SECURITY REQUIREMENTS

All of the following are mandatory — no exceptions, no deferral to "future work":

### Authentication & Sessions
- JWT access tokens: **15-minute expiry**, signed with **HS256** (configurable key from env var)
- Refresh tokens: **7-day expiry**, **single-use with rotation** — each refresh issues a new token and revokes the old one atomically
- Refresh token stored **SHA-256 hashed** in DB — never plaintext
- Account lockout: **5 consecutive failures → 15-minute lockout** (`LockoutEnd` on User entity)
- TOTP MFA (RFC 6238) via `OtpNet`
- Backup codes: **8 single-use** codes on MFA setup, stored as BCrypt hashes
- Impersonation: Admin-only; impersonated JWT carries both `sub` (impersonated) and `original_sub` (admin) claims; all audit logs during impersonation record the original admin userId

### Authorization
- Role hierarchy: `Taxpayer < Officer < Supervisor < Admin`
- Resource ownership enforced at **handler level** (not just controller `[Authorize]`): taxpayers can only read/write their own complaints, documents, appointments
- Permission overrides: `UserPermissionOverride` allows granting or denying specific permissions to individual users, overriding role defaults
- `ICurrentUserService.HasPermission(string code)` checks role permissions + overrides

### Input Validation
- FluentValidation on every command — return **422 Unprocessable Entity** with error details
- Sanitise all free-text fields (strip script/HTML content via `HtmlSanitizer` or equivalent)
- File upload: validate **magic bytes** (not just extension); max 10MB; allowed: PDF, DOC, DOCX, XLS, XLSX, PNG, JPG, JPEG
- TIN/NIN format validation per configured regex (injectable via `SystemSettings`)

### Transport & Headers
- Enforce HTTPS in production; middleware redirects HTTP
- `SecurityHeadersMiddleware` sets on every response:
  - `Strict-Transport-Security: max-age=31536000; includeSubDomains`
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Content-Security-Policy: default-src 'self'`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy: geolocation=(), microphone=()`
  - Remove `Server` and `X-Powered-By` headers

### Rate Limiting (using `System.Threading.RateLimiting`)
- Global: **1,000 req/min per IP**
- Auth endpoints (`/auth/login`, `/auth/register`, `/auth/forgot-password`): **10 req/min per IP**
- File upload: **20 req/min per authenticated user**

### Audit Trail
- `AuditBehaviour` pipeline step auto-creates `AuditLog` for every state-changing command
- Captures: `EntityType`, `EntityId`, `Action`, `OldValues` (JSON), `NewValues` (JSON), `UserId`, `IpAddress`, `UserAgent`, `Timestamp`
- `AuditLog` is **append-only** — no update/delete endpoints

### SQL Safety
- EF Core LINQ exclusively — no string-interpolated `FromSqlRaw`
- Any raw SQL uses `FromSqlRaw` with `SqlParameter` typed parameters

---

## DATABASE — MSSQL CONFIGURATION

- `EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)`
- `CommandTimeout(60)`
- `newsequentialid()` default for all GUID PKs
- `datetime2(7)` for all date/time columns
- `decimal(18,2)` for all financial fields
- Collation `Latin1_General_CI_AI` on text columns used for search

### Seeding (runs on first startup if tables empty)
1. **Roles**: Admin, Supervisor, Officer, Taxpayer
2. **Permissions**: full list covering all 207 endpoints grouped by feature
3. **Role-Permission assignments**: sensible defaults per role
4. **Departments**: one default "General" department
5. **Lookup data**: LeaveTypes, ComplaintCategories, TaxTypes, DocumentTypes, CommunicationChannels
6. **Default Admin user**: from `appsettings.json:DefaultAdmin` section

---

## BACKGROUND JOBS (HANGFIRE)

| Job Class | Schedule | Logic |
|---|---|---|
| `SlaBreachNotificationJob` | Daily 06:00 | Query open cases where `DueDate < UtcNow`; create Notification for officer + supervisor; dispatch `case.sla_breached` webhook |
| `SendScheduledReportJob` | Per cron on each `ScheduledReport` | Run report query; email as CSV/Excel attachment; update `LastRunAt` |
| `WebhookRetryJob` | Every 5 minutes | Retry `WebhookDelivery` records where `Succeeded=false` and `AttemptCount < 5`; exponential backoff: `2^attempt` seconds |
| `CleanupExpiredTokensJob` | Daily 02:00 | Hard-delete expired `RefreshToken` and `MfaToken` rows |
| `NotificationCleanupJob` | Weekly Sunday 03:00 | Soft-delete read Notifications older than 90 days |

---

## WEBHOOK SYSTEM

- `WebhookDispatchService`: after domain event, query `WebhookSubscription` by `EventType`; HTTP POST to `Url` with HMAC-SHA256 `X-TaxOmbud-Signature: sha256=<hex>` header computed against the stored `SecretHash`
- Payload: `{ "event": "complaint.submitted", "timestamp": "2024-01-01T00:00:00Z", "data": { ... } }`
- Timeout: 10 seconds per delivery attempt
- Record every attempt in `WebhookDelivery` (success or failure)
- `POST /webhooks/{id}/rotate-secret` replaces the stored secret hash; returns the new plaintext secret once only

**Event types:** `complaint.submitted` `complaint.assigned` `complaint.escalated` `complaint.closed` `case.opened` `case.assigned` `case.transitioned` `case.closed` `case.sla_breached` `appeal.submitted` `appeal.upheld` `appeal.dismissed` `taxpayer.registered` `taxpayer.verified` `leave.approved` `loan.approved`

---

## TESTING REQUIREMENTS

### Unit Tests (`TaxOmbud.Domain.Tests`, `TaxOmbud.Application.Tests`)
- Every Command handler tested in isolation (mock all interfaces)
- Every Query handler tested in isolation
- Every domain entity state-transition method tested (valid + invalid transitions)
- Every FluentValidation validator tested (valid + each invalid case)
- Target: **≥ 80% line coverage** on Domain + Application layers

### Integration Tests (`TaxOmbud.API.IntegrationTests`)
- Use `WebApplicationFactory<Program>` against SQL Server LocalDB or Testcontainers
- Must include end-to-end tests for:
  - Full auth flow: register → verify email → login → MFA → refresh → logout
  - Full complaint lifecycle: submit → assign → escalate → close
  - Full case lifecycle: open → assign → transition stages → recommend → approve closure → close
  - Full HR cycle: create staff profile → request leave → approve leave
  - Document upload + download-url
  - Rate limiting on `/auth/login`
  - Impersonation: start → confirm claims → stop

---

## `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaxOmbud;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "TaxOmbud.API",
    "Audience": "TaxOmbud.Client",
    "SecretKey": "CHANGE_IN_PRODUCTION_MIN_32_CHARS",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Email": {
    "Host": "smtp.example.com",
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
    "Password": "CHANGE_IN_PRODUCTION",
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
  },
  "NinVerification": {
    "BaseUrl": "https://nin-verification.example.gov.ng",
    "ApiKey": "CHANGE_IN_PRODUCTION"
  },
  "Impersonation": {
    "AllowedRoles": ["Admin"],
    "TokenExpiryMinutes": 60
  }
}
```

---

## DOCKER

### `Dockerfile` (multi-stage)
1. Stage `build`: `mcr.microsoft.com/dotnet/sdk:9.0` — restore, build, publish to `/app/publish`
2. Stage `runtime`: `mcr.microsoft.com/dotnet/aspnet:9.0` — copy `/app/publish`, run as `USER app` (non-root), `EXPOSE 8080`, `ENTRYPOINT`

### `docker-compose.yml`
```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong!Passw0rd"
      ACCEPT_EULA: "Y"
    volumes:
      - sqldata:/var/opt/mssql
    ports: ["1433:1433"]

  api:
    build: .
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: "Server=db;Database=TaxOmbud;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
      Jwt__SecretKey: "dev-secret-key-min-32-characters!!"
    ports: ["8080:8080"]
    depends_on: [db]

volumes:
  sqldata:
```

---

## QUALITY STANDARDS (enforced via project settings)

```xml
<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <AnalysisMode>All</AnalysisMode>
</PropertyGroup>
```

- No `async void` — always `async Task`
- No `DateTime.Now` — always `IDateTimeService.UtcNow`
- No magic strings — all constants in `static class Constants`
- `sealed` on all classes not designed for inheritance
- `record` types for DTOs, Commands, Queries, domain events
- All `IDisposable` resources wrapped in `using` declarations
- `CancellationToken` threaded through every async method to EF and IO

---

## DELIVERABLES CHECKLIST

Do not consider the task complete until every item is checked:

- [ ] `TaxOmbud.sln` with all 7 projects, correct project references
- [ ] All Domain entities (55+), enums (18), value objects (4), domain events (16), exceptions (5)
- [ ] All Application commands + queries + handlers + validators + DTOs for all 20 feature modules
- [ ] All 4 MediatR pipeline behaviours
- [ ] All Infrastructure EF entity configurations (one file per entity)
- [ ] EF Core initial migration — applies cleanly on empty MSSQL
- [ ] Database seeder (roles, permissions, lookups, default admin, department)
- [ ] All Infrastructure service implementations (TokenService, EmailService, StorageService, MfaService, NinVerificationService, ImpersonationService, WebhookDispatchService, ReferenceNumberService, AuditService)
- [ ] All 20 API controllers with all **207 endpoints** implemented
- [ ] `ApiControllerBase` with `HandleResult<T>`
- [ ] All 4 middleware classes
- [ ] `Program.cs` — full pipeline + DI registration
- [ ] `appsettings.json` + `appsettings.Development.json`
- [ ] 5 Hangfire background jobs
- [ ] Webhook dispatch + HMAC signing + delivery recording
- [ ] Unit tests ≥ 80% coverage on Domain + Application
- [ ] Integration tests for all 5 specified workflows
- [ ] Multi-stage `Dockerfile`
- [ ] `docker-compose.yml`
- [ ] `README.md` with ASCII architecture diagram, setup steps, env var table, role-permission matrix

**Do not stop until every checkbox above is complete.**
