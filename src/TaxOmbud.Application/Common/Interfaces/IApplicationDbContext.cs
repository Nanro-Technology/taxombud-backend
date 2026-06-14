using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Common.Interfaces;

/// <summary>
/// Defines the data access contract used by the Application layer.
/// Infrastructure implements this — Application only sees this interface.
/// </summary>
public interface IApplicationDbContext
{
    // Identity & RBAC
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserPermissionOverride> UserPermissionOverrides { get; }
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
    DbSet<Case> Cases { get; }
    DbSet<CaseNote> CaseNotes { get; }
    DbSet<CaseFinding> CaseFindings { get; }
    DbSet<CaseMilestone> CaseMilestones { get; }
    DbSet<CaseCommunicationLog> CaseCommunicationLogs { get; }
    DbSet<CaseTask> CaseTasks { get; }

    // Documents
    DbSet<Document> Documents { get; }
    DbSet<DocumentVersion> DocumentVersions { get; }

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

        DbSet<TaxOmbud.Domain.Entities.Operations.Project> Projects { get; }
    DbSet<TaxOmbud.Domain.Entities.Operations.ProjectTask> ProjectTasks { get; }
    DbSet<TaxOmbud.Domain.Entities.Operations.InventoryItem> InventoryItems { get; }
    DbSet<TaxOmbud.Domain.Entities.Operations.VendorContact> VendorContacts { get; }
    DbSet<TaxOmbud.Domain.Entities.Finance.Quote> Quotes { get; }
    DbSet<TaxOmbud.Domain.Entities.Finance.Contract> Contracts { get; }
    DbSet<TaxOmbud.Domain.Entities.Finance.QuoteItem> QuoteItems { get; }
    DbSet<TaxOmbud.Domain.Entities.Finance.ContractReview> ContractReviews { get; }
    DbSet<TaxOmbud.Domain.Entities.Finance.Invoice> Invoices { get; }
    DbSet<TaxOmbud.Domain.Entities.Finance.InvoiceItem> InvoiceItems { get; }
    DbSet<TaxOmbud.Domain.Entities.System.Announcement> Announcements { get; }
    DbSet<TaxOmbud.Domain.Entities.Appointments.CalendarEvent> CalendarEvents { get; }
    DbSet<TaxOmbud.Domain.Entities.Communications.AgentChat> AgentChats { get; }
    DbSet<TaxOmbud.Domain.Entities.System.AnnouncementReadReceipt> AnnouncementReadReceipts { get; }
    DbSet<TaxOmbud.Domain.Entities.System.DashboardWidget> DashboardWidgets { get; }
    DbSet<TaxOmbud.Domain.Entities.System.UserDashboardLayout> UserDashboardLayouts { get; }
    DbSet<TaxOmbud.Domain.Entities.Communications.MailboxMessage> MailboxMessages { get; }
    DbSet<TaxOmbud.Domain.Entities.Communications.MailboxRecipient> MailboxRecipients { get; }
    DbSet<TaxOmbud.Domain.Entities.Communications.MailboxAttachment> MailboxAttachments { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.PerformanceCycle> PerformanceCycles { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.PerformanceGoal> PerformanceGoals { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.PerformanceReview> PerformanceReviews { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.DisciplinaryCase> DisciplinaryCases { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.ExitRecord> ExitRecords { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.BenefitType> BenefitTypes { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.EmployeeBenefit> EmployeeBenefits { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.LeaveTypeEntity> LeaveTypeEntities { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.LeaveBalance> LeaveBalances { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.StatutoryDeduction> StatutoryDeductions { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.StatutoryRule> StatutoryRules { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.PayoutProvider> PayoutProviders { get; }
    DbSet<TaxOmbud.Domain.Entities.Operations.ProjectMember> ProjectMembers { get; }
    DbSet<TaxOmbud.Domain.Entities.Operations.Ticket> Tickets { get; }
    DbSet<TaxOmbud.Domain.Entities.Operations.Visitor> Visitors { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.AttendanceLog> AttendanceLogs { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.Holiday> Holidays { get; }
    DbSet<TaxOmbud.Domain.Entities.Communications.AgentChatMessage> AgentChatMessages { get; }
    DbSet<TaxOmbud.Domain.Entities.Communications.AgentChatPreference> AgentChatPreferences { get; }
    DbSet<TaxOmbud.Domain.Entities.Communications.SmsMessage> SmsMessages { get; }
    DbSet<TaxOmbud.Domain.Entities.Crm.Organization> Organizations { get; }
    DbSet<TaxOmbud.Domain.Entities.Crm.Interaction> Interactions { get; }
    DbSet<TaxOmbud.Domain.Entities.Crm.Call> Calls { get; }
    DbSet<TaxOmbud.Domain.Entities.Hr.TimeLog> TimeLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}









