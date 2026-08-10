using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Crm;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Finance;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Operations;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Domain.Entities.Taxpayers;

namespace TaxOmbud.Application.Interfaces.Persistence;

/// <summary>
/// Defines the data access contract used by the Application layer.
/// Infrastructure implements this — Application only sees this interface.
/// </summary>
public interface IApplicationDbContext
{
    // Identity & RBAC
    DbSet<User> Users { get; }
    DbSet<Role> CustomRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<MfaToken> MfaTokens { get; }
    DbSet<Department> Departments { get; }

    // Taxpayer
    DbSet<Taxpayer> Taxpayers { get; }
    DbSet<TaxpayerProfile> TaxpayerProfiles { get; }

    // Officers
    DbSet<OfficerProfile> OfficerProfiles { get; }
    DbSet<OfficerCaseload> OfficerCaseloads { get; }

    // Complaints & Cases
    DbSet<Complaint> Complaints { get; }
    DbSet<ComplaintNote> ComplaintNotes { get; }
    DbSet<ComplaintStatusHistory> ComplaintStatusHistory { get; }
    DbSet<ComplaintLink> ComplaintLinks { get; }
    DbSet<CallCenterRecord> CallCenterRecords { get; }
    DbSet<Case> Cases { get; }
    DbSet<CaseNote> CaseNotes { get; }
    DbSet<CaseFinding> CaseFindings { get; }
    DbSet<CaseMilestone> CaseMilestones { get; }
    DbSet<CaseCommunicationLog> CaseCommunicationLogs { get; }
    DbSet<CaseTask> CaseTasks { get; }
    DbSet<AdmissibilityAssessment> AdmissibilityAssessments { get; }
    DbSet<MediationLog> MediationLogs { get; }
    DbSet<QualityAssuranceReview> QualityAssuranceReviews { get; }
    DbSet<CaseDecision> CaseDecisions { get; }


    // Workflow Engine
    DbSet<TaxOmbud.Domain.Entities.Workflows.Workflow> Workflows { get; }
    DbSet<TaxOmbud.Domain.Entities.Workflows.WorkflowVersion> WorkflowVersions { get; }
    DbSet<TaxOmbud.Domain.Entities.Workflows.WorkflowLevel> WorkflowLevels { get; }
    DbSet<TaxOmbud.Domain.Entities.Workflows.WorkflowInstance> WorkflowInstances { get; }
    DbSet<TaxOmbud.Domain.Entities.Workflows.WorkflowInstanceLevel> WorkflowInstanceLevels { get; }
    DbSet<TaxOmbud.Domain.Entities.Workflows.CaseApprovalTask> CaseApprovalTasks { get; }
    DbSet<TaxOmbud.Domain.Entities.Workflows.CaseWorkflowAuditLog> CaseWorkflowAuditLogs { get; }

    // Documents
    DbSet<Document> Documents { get; }
    DbSet<DocumentVersion> DocumentVersions { get; }
    DbSet<TaxOmbud.Domain.Entities.Documents.UserFile> UserFiles { get; }
    DbSet<TaxOmbud.Domain.Entities.Documents.SignRequest> SignRequests { get; }
    DbSet<TaxOmbud.Domain.Entities.Documents.PublicFileRequest> PublicFileRequests { get; }
    DbSet<TaxOmbud.Domain.Entities.Documents.PublicFileRequestUpload> PublicFileRequestUploads { get; }

    // Secured Filing
    DbSet<TaxOmbud.Domain.Entities.SecuredFiling.FilingFolder> FilingFolders { get; }
    DbSet<TaxOmbud.Domain.Entities.SecuredFiling.FilingDocument> FilingDocuments { get; }
    DbSet<TaxOmbud.Domain.Entities.SecuredFiling.FilingCategory> FilingCategories { get; }
    DbSet<TaxOmbud.Domain.Entities.SecuredFiling.FilingInboxRouting> FilingInboxRoutings { get; }

    // Communications
    DbSet<CommunicationLog> CommunicationLogs { get; }
    DbSet<CommunicationTemplate> CommunicationTemplates { get; }

    // Appeals
    DbSet<Appeal> Appeals { get; }
    DbSet<AppealGroundPoint> AppealGroundPoints { get; }

    // Appointments
    DbSet<Appointment> Appointments { get; }

    // Notifications
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }

    // System
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<FeatureFlag> FeatureFlags { get; }
    DbSet<WebhookSubscription> WebhookSubscriptions { get; }
    DbSet<ScheduledReport> ScheduledReports { get; }

    // HR & Payroll
    DbSet<Account> Accounts { get; }
    DbSet<StaffProfile> StaffProfiles { get; }
    DbSet<PayGrade> PayGrades { get; }
    DbSet<SalaryProfile> SalaryProfiles { get; }
    DbSet<PayrollPeriod> PayrollPeriods { get; }
    DbSet<PayrollRun> PayrollRuns { get; }
    DbSet<PayrollEntry> PayrollEntries { get; }
    DbSet<Remittance> Remittances { get; }
    DbSet<EmployeeWallet> EmployeeWallets { get; }
    DbSet<WalletTransaction> WalletTransactions { get; }
    DbSet<LoanRequest> LoanRequests { get; }
    DbSet<EwaRequest> EwaRequests { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<Competency> Competencies { get; }
    DbSet<ReviewTemplate> ReviewTemplates { get; }

    DbSet<Project> Projects { get; }
    DbSet<ProjectTask> ProjectTasks { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<VendorContact> VendorContacts { get; }
    DbSet<Quote> Quotes { get; }
    DbSet<Contract> Contracts { get; }
    DbSet<QuoteItem> QuoteItems { get; }
    DbSet<ContractReview> ContractReviews { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Announcement> Announcements { get; }
    DbSet<CalendarEvent> CalendarEvents { get; }
    DbSet<AgentChat> AgentChats { get; }
    DbSet<AnnouncementReadReceipt> AnnouncementReadReceipts { get; }
    DbSet<DashboardWidget> DashboardWidgets { get; }
    DbSet<UserDashboardLayout> UserDashboardLayouts { get; }
    DbSet<MailboxMessage> MailboxMessages { get; }
    DbSet<MailboxRecipient> MailboxRecipients { get; }
    DbSet<MailboxAttachment> MailboxAttachments { get; }
    DbSet<PerformanceCycle> PerformanceCycles { get; }
    DbSet<PerformanceGoal> PerformanceGoals { get; }
    DbSet<PerformanceReview> PerformanceReviews { get; }
    DbSet<DisciplinaryCase> DisciplinaryCases { get; }
    DbSet<ExitRecord> ExitRecords { get; }
    DbSet<BenefitType> BenefitTypes { get; }
    DbSet<EmployeeBenefit> EmployeeBenefits { get; }
    DbSet<LeaveTypeEntity> LeaveTypeEntities { get; }
    DbSet<LeaveBalance> LeaveBalances { get; }
    DbSet<StatutoryDeduction> StatutoryDeductions { get; }
    DbSet<StatutoryRule> StatutoryRules { get; }
    DbSet<PayoutProvider> PayoutProviders { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<Visitor> Visitors { get; }
    DbSet<AttendanceLog> AttendanceLogs { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<AgentChatMessage> AgentChatMessages { get; }
    DbSet<AgentChatPreference> AgentChatPreferences { get; }
    DbSet<SmsMessage> SmsMessages { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<Interaction> Interactions { get; }
    DbSet<Call> Calls { get; }
    DbSet<TimeLog> TimeLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
