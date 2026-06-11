using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Common;
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
using TaxOmbud.Domain.ValueObjects;

namespace TaxOmbud.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public ApplicationDbContext(
        DbContextOptions options,
        ICurrentUser currentUser,
        IMediator mediator)
        : base(options)
    {
        _currentUser = currentUser;
        _mediator = mediator;
    }

    // ─── Identity & RBAC ─────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MfaToken> MfaTokens => Set<MfaToken>();
    public DbSet<Department> Departments => Set<Department>();

    // ─── Taxpayer ─────────────────────────────────────────────────────────────
    public DbSet<Taxpayer> Taxpayers => Set<Taxpayer>();
    public DbSet<TaxpayerProfile> TaxpayerProfiles => Set<TaxpayerProfile>();

    // ─── Officer ──────────────────────────────────────────────────────────────
    public DbSet<OfficerProfile> OfficerProfiles => Set<OfficerProfile>();
    public DbSet<OfficerCaseload> OfficerCaseloads => Set<OfficerCaseload>();

    // ─── Complaints & Cases ───────────────────────────────────────────────────
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintNote> ComplaintNotes => Set<ComplaintNote>();
    public DbSet<ComplaintStatusHistory> ComplaintStatusHistory => Set<ComplaintStatusHistory>();
    public DbSet<ComplaintLink> ComplaintLinks => Set<ComplaintLink>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseNote> CaseNotes => Set<CaseNote>();
    public DbSet<CaseFinding> CaseFindings => Set<CaseFinding>();
    public DbSet<CaseMilestone> CaseMilestones => Set<CaseMilestone>();
    public DbSet<CaseCommunicationLog> CaseCommunicationLogs => Set<CaseCommunicationLog>();

    // ─── Documents ────────────────────────────────────────────────────────────
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

    // ─── Communications ───────────────────────────────────────────────────────
    public DbSet<CommunicationLog> CommunicationLogs => Set<CommunicationLog>();
    public DbSet<CommunicationTemplate> CommunicationTemplates => Set<CommunicationTemplate>();

    // ─── Appeals ──────────────────────────────────────────────────────────────
    public DbSet<Appeal> Appeals => Set<Appeal>();
    public DbSet<AppealGroundPoint> AppealGroundPoints => Set<AppealGroundPoint>();

    // ─── Appointments ─────────────────────────────────────────────────────────
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // ─── Notifications ────────────────────────────────────────────────────────
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    // ─── System ───────────────────────────────────────────────────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();

    // ─── HR & Payroll ─────────────────────────────────────────────────────────
    public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
    public DbSet<PayGrade> PayGrades => Set<PayGrade>();
    public DbSet<SalaryProfile> SalaryProfiles => Set<SalaryProfile>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
    public DbSet<Remittance> Remittances => Set<Remittance>();
    public DbSet<EmployeeWallet> EmployeeWallets => Set<EmployeeWallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<LoanRequest> LoanRequests => Set<LoanRequest>();
    public DbSet<EwaRequest> EwaRequests => Set<EwaRequest>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    // ─── EF Model ─────────────────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Email>().HaveConversion<EmailValueConverter>();
        configurationBuilder.Properties<TaxIdentificationNumber>().HaveConversion<TaxIdentificationNumberValueConverter>();
        configurationBuilder.Properties<ReferenceNumber>().HaveConversion<ReferenceNumberValueConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    // ─── Audit + Domain Events ────────────────────────────────────────────────
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
                case EntityState.Deleted when entry.Entity is ISoftDelete sd:
                    entry.State = EntityState.Modified;
                    sd.IsDeleted = true;
                    sd.DeletedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }

        // Collect and dispatch domain events before persisting
        var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var events = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        foreach (var ev in events)
            await _mediator.Publish(ev, cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }
}

public class EmailValueConverter : ValueConverter<Email, string>
{
    public EmailValueConverter()
        : base(
            email => email.Value,
            value => new Email(value))
    {
    }
}

public class TaxIdentificationNumberValueConverter : ValueConverter<TaxIdentificationNumber, string>
{
    public TaxIdentificationNumberValueConverter()
        : base(
            tin => tin.Value,
            value => new TaxIdentificationNumber(value))
    {
    }
}

public class ReferenceNumberValueConverter : ValueConverter<ReferenceNumber, string>
{
    public ReferenceNumberValueConverter()
        : base(
            refNum => refNum.Value,
            value => ReferenceNumber.From(value))
    {
    }
}
