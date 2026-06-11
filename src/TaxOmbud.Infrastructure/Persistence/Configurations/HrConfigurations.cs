using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Infrastructure.Persistence.Configurations;

public class StaffProfileConfiguration : IEntityTypeConfiguration<StaffProfile>
{
    public void Configure(EntityTypeBuilder<StaffProfile> builder)
    {
        builder.ToTable("StaffProfiles");
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.EmploymentStatus).HasMaxLength(50).IsRequired();
        builder.Property(sp => sp.Nationality).HasMaxLength(100).IsRequired();
        builder.Property(sp => sp.MaritalStatus).HasMaxLength(50).IsRequired();
        builder.Property(sp => sp.EmergencyContact).HasMaxLength(500).IsRequired();
        builder.Property(sp => sp.BankAccountNo).HasMaxLength(50).IsRequired();
        builder.Property(sp => sp.BankId).HasMaxLength(50).IsRequired();
        builder.Property(sp => sp.NextOfKin).HasMaxLength(500).IsRequired();

        builder.HasQueryFilter(sp => !sp.IsDeleted);

        builder.HasOne(sp => sp.User)
            .WithMany()
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayGradeConfiguration : IEntityTypeConfiguration<PayGrade>
{
    public void Configure(EntityTypeBuilder<PayGrade> builder)
    {
        builder.ToTable("PayGrades");
        builder.HasKey(pg => pg.Id);

        builder.Property(pg => pg.Name).HasMaxLength(100).IsRequired();
        builder.Property(pg => pg.BasicSalaryBand).HasMaxLength(100).IsRequired();

        builder.HasQueryFilter(pg => !pg.IsDeleted);
    }
}

public class SalaryProfileConfiguration : IEntityTypeConfiguration<SalaryProfile>
{
    public void Configure(EntityTypeBuilder<SalaryProfile> builder)
    {
        builder.ToTable("SalaryProfiles");
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Basic).HasPrecision(18, 2);

        builder.HasQueryFilter(sp => !sp.IsDeleted);

        builder.HasOne(sp => sp.User)
            .WithMany()
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.ToTable("PayrollPeriods");
        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.Name).HasMaxLength(100).IsRequired();
        builder.Property(pp => pp.Status).HasMaxLength(50).IsRequired();

        builder.HasQueryFilter(pp => !pp.IsDeleted);
    }
}

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns");
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.RunType).HasMaxLength(50).IsRequired();
        builder.Property(pr => pr.Status).HasMaxLength(50).IsRequired();

        builder.Property(pr => pr.TotalGross).HasPrecision(18, 2);
        builder.Property(pr => pr.TotalNet).HasPrecision(18, 2);
        builder.Property(pr => pr.TotalStatutory).HasPrecision(18, 2);

        builder.HasQueryFilter(pr => !pr.IsDeleted);

        builder.HasOne(pr => pr.Period)
            .WithMany()
            .HasForeignKey(pr => pr.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.ApprovedByUser)
            .WithMany()
            .HasForeignKey(pr => pr.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PayrollEntryConfiguration : IEntityTypeConfiguration<PayrollEntry>
{
    public void Configure(EntityTypeBuilder<PayrollEntry> builder)
    {
        builder.ToTable("PayrollEntries");
        builder.HasKey(pe => pe.Id);

        builder.Property(pe => pe.Basic).HasPrecision(18, 2);
        builder.Property(pe => pe.Allowances).HasPrecision(18, 2);
        builder.Property(pe => pe.Deductions).HasPrecision(18, 2);
        builder.Property(pe => pe.Paye).HasPrecision(18, 2);
        builder.Property(pe => pe.Pension).HasPrecision(18, 2);
        builder.Property(pe => pe.Nhf).HasPrecision(18, 2);
        builder.Property(pe => pe.OtherStatutory).HasPrecision(18, 2);
        builder.Property(pe => pe.Gross).HasPrecision(18, 2);
        builder.Property(pe => pe.Net).HasPrecision(18, 2);

        builder.Property(pe => pe.PaymentStatus).HasMaxLength(50).IsRequired();

        builder.HasOne(pe => pe.Run)
            .WithMany()
            .HasForeignKey(pe => pe.RunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pe => pe.User)
            .WithMany()
            .HasForeignKey(pe => pe.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RemittanceConfiguration : IEntityTypeConfiguration<Remittance>
{
    public void Configure(EntityTypeBuilder<Remittance> builder)
    {
        builder.ToTable("Remittances");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.DeductionType).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Amount).HasPrecision(18, 2);
        builder.Property(r => r.Status).HasMaxLength(50).IsRequired();
        builder.Property(r => r.ReferenceNumber).HasMaxLength(100);

        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasOne(r => r.Run)
            .WithMany()
            .HasForeignKey(r => r.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeWalletConfiguration : IEntityTypeConfiguration<EmployeeWallet>
{
    public void Configure(EntityTypeBuilder<EmployeeWallet> builder)
    {
        builder.ToTable("EmployeeWallets");
        builder.HasKey(ew => ew.Id);

        builder.Property(ew => ew.BalanceNgn).HasPrecision(18, 2);

        builder.HasOne(ew => ew.User)
            .WithMany()
            .HasForeignKey(ew => ew.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ew => ew.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");
        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.Type).HasMaxLength(50).IsRequired();
        builder.Property(wt => wt.Amount).HasPrecision(18, 2);
        builder.Property(wt => wt.Reference).HasMaxLength(100).IsRequired();
    }
}

public class LoanRequestConfiguration : IEntityTypeConfiguration<LoanRequest>
{
    public void Configure(EntityTypeBuilder<LoanRequest> builder)
    {
        builder.ToTable("LoanRequests");
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.Amount).HasPrecision(18, 2);
        builder.Property(lr => lr.Purpose).HasMaxLength(1000).IsRequired();
        builder.Property(lr => lr.Status).HasMaxLength(50).IsRequired();

        builder.HasQueryFilter(lr => !lr.IsDeleted);

        builder.HasOne(lr => lr.User)
            .WithMany()
            .HasForeignKey(lr => lr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EwaRequestConfiguration : IEntityTypeConfiguration<EwaRequest>
{
    public void Configure(EntityTypeBuilder<EwaRequest> builder)
    {
        builder.ToTable("EwaRequests");
        builder.HasKey(er => er.Id);

        builder.Property(er => er.Amount).HasPrecision(18, 2);
        builder.Property(er => er.Status).HasMaxLength(50).IsRequired();

        builder.HasQueryFilter(er => !er.IsDeleted);

        builder.HasOne(er => er.User)
            .WithMany()
            .HasForeignKey(er => er.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(er => er.RecoveredInPeriod)
            .WithMany()
            .HasForeignKey(er => er.RecoveredInPeriodId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.LeaveType).HasMaxLength(50).IsRequired();
        builder.Property(lr => lr.Status).HasMaxLength(50).IsRequired();
        builder.Property(lr => lr.SupervisorNote).HasMaxLength(1000);

        builder.HasQueryFilter(lr => !lr.IsDeleted);

        builder.HasOne(lr => lr.User)
            .WithMany()
            .HasForeignKey(lr => lr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lr => lr.ApproverUser)
            .WithMany()
            .HasForeignKey(lr => lr.ApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
