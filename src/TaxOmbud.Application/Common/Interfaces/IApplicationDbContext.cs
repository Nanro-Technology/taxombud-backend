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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
