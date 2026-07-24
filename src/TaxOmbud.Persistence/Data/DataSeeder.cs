using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Persistence.Data;

/// <summary>
/// Seeds all reference data (permissions, roles, role-permissions, admin user) on startup.
/// Idempotent — safe to run multiple times.
/// Now uses UserManager<User> to create the seed user so that Identity's password hashing,
/// normalisation, and security stamp are all properly initialised.
/// </summary>
public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAllAsync()
    {
        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedRolePermissionsAsync();
        await SeedDepartmentsAsync();
        await SeedPayGradesAsync();
        await SeedPerformanceSettingsAsync();
        await SeedUsersAsync();
        await SeedStaffProfilesAsync();
        await SeedPayrollAsync();
        await SeedPeopleOpsAsync();
        await SeedAccountsAsync();
        await SeedTaxpayersAsync();
        await SeedKnowledgeCenterAsync();
        await SeedChatsAsync();
        await SeedSystemSettingsAsync();
        await SeedWorkflowsAsync();
    }

    // ─── 1. Permissions ──────────────────────────────────────────────────────────
    /// <summary>
    /// Auto-generates one Permission row for every Modules × PermissionAction combination.
    /// New modules or actions added to the enums will be seeded on next startup.
    /// </summary>
    private async Task SeedPermissionsAsync()
    {
        var existing = await _context.Permissions.ToListAsync();
        var toAdd = new List<Permission>();

        foreach (Modules module in Enum.GetValues(typeof(Modules)))
        {
            foreach (PermissionAction action in Enum.GetValues(typeof(PermissionAction)))
            {
                if (!existing.Any(p => p.Module == module && p.Action == action))
                {
                    toAdd.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Module = module,
                        Action = action,
                        CreatedAt = DateTime.Now.ToUniversalTime()
                    });
                }
            }
        }

        if (toAdd.Any())
        {
            await _context.Permissions.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Seeded {Count} new permissions", toAdd.Count);
        }
    }

    // ─── 2. Roles ────────────────────────────────────────────────────────────────
    private async Task SeedRolesAsync()
    {
        // NOTE: These roles are ONLY for StaffUser accounts.
        // Taxpayers and Guests do NOT have roles — their UserType is their identity.
        var roleDefs = new[]
        {
            (RoleConstants.SuperAdmin,    "Full system access with all permissions",    true),
            (RoleConstants.Admin,         "Administrative access",                      true),
            (RoleConstants.Director,      "Directorate director",                       false),
            (RoleConstants.Manager,       "Department manager",                         false),
            (RoleConstants.SeniorOfficer, "Senior officer with escalation rights",      false),
            (RoleConstants.Officer,       "Case management officer",                    false),
            (RoleConstants.Auditor,       "Read-only audit access",                     false),
            (RoleConstants.HrManager,     "HR and payroll manager",                     false),
            (RoleConstants.Finance,       "Finance and remittance officer",             false),
        };

        foreach (var (name, description, isSystem) in roleDefs)
        {
            if (!await _context.CustomRoles.AnyAsync(r => r.Name == name))
            {
                await _context.CustomRoles.AddAsync(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    IsSystemRole = isSystem,
                    IsActive = true,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Roles seeded");
    }

    // ─── 3. Role → Permission assignments ────────────────────────────────────────
    /// <summary>Assigns all permissions to SuperAdmin; other roles start with no permissions (configured via UI).</summary>
    private async Task SeedRolePermissionsAsync()
    {
        var superAdmin = await _context.CustomRoles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == RoleConstants.SuperAdmin);

        if (superAdmin is null) return;

        var allPermissions = await _context.Permissions.ToListAsync();
        var toAdd = new List<RolePermission>();

        foreach (var permission in allPermissions)
        {
            if (!superAdmin.RolePermissions.Any(rp => rp.PermissionId == permission.Id))
            {
                toAdd.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = superAdmin.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                });
            }
        }

        if (toAdd.Any())
        {
            await _context.RolePermissions.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Assigned {Count} permissions to Super Admin", toAdd.Count);
        }
    }

    // ─── 4. Default Super Admin user ─────────────────────────────────────────────
    /// <summary>
    /// Creates the seeded Super Admin using UserManager so that Identity's password hashing,
    /// security stamp, normalized email, and lockout fields are all correctly initialised.
    /// The first-login password must be changed immediately.
    /// </summary>
    private async Task SeedUsersAsync()
    {
        const string adminEmail = "admin@taxombud.gov.ng";
        const string defaultPassword = "Admin@TaxOmbud2025!";

        var existingAdmin = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        var ictDept = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "ICT");

        if (existingAdmin is null)
        {
            var superAdminRole = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Name == RoleConstants.SuperAdmin);
            if (superAdminRole is not null)
            {
                var admin = User.Create(
                    "System",
                    "Administrator",
                    new Email(adminEmail),
                    "+2349052129949",
                    UserType.StaffUser);

                admin.AssignRole(superAdminRole.Id);
                admin.UpdateProfile("System", "Administrator", "+2349052129949", "S.A on ICT");
                admin.SetEmploymentType("Full-Time");
                if (ictDept is not null)
                {
                    admin.SetDepartment(ictDept.Id);
                }

                var result = await _userManager.CreateAsync(admin, defaultPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("✓ Default Super Admin seeded: {Email}", adminEmail);
                    _logger.LogWarning("⚠ Change the default admin password immediately after first login!");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("✗ Failed to seed Super Admin: {Errors}", errors);
                }
            }
        }
        else
        {
            bool needsUpdate = false;
            // Backfill normalized fields for existing records
            if (string.IsNullOrEmpty(existingAdmin.NormalizedEmail) || string.IsNullOrEmpty(existingAdmin.NormalizedUserName))
            {
                existingAdmin.NormalizedEmail = _userManager.NormalizeEmail(adminEmail);
                existingAdmin.NormalizedUserName = _userManager.NormalizeName(adminEmail);
                needsUpdate = true;
                _logger.LogInformation("✓ Backfilled normalized email/username fields for: {Email}", adminEmail);
            }

            // Backfill default profile details if empty
            if (string.IsNullOrEmpty(existingAdmin.Phone))
            {
                existingAdmin.Phone = "+2349052129949";
                needsUpdate = true;
            }
            if (string.IsNullOrEmpty(existingAdmin.JobTitle))
            {
                existingAdmin.JobTitle = "S.A on ICT";
                needsUpdate = true;
            }
            if (string.IsNullOrEmpty(existingAdmin.EmploymentType))
            {
                existingAdmin.EmploymentType = "Full-Time";
                needsUpdate = true;
            }
            if (existingAdmin.DepartmentId == null && ictDept is not null)
            {
                existingAdmin.DepartmentId = ictDept.Id;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _userManager.UpdateAsync(existingAdmin);
                _logger.LogInformation("✓ Backfilled seeded admin profile details: Phone, Job Title, Employment Type, Department.");
            }
        }

        // No dummy staff users are seeded — all staff accounts are created through
        // the HR onboarding workflow by administrators.
    }

    // ─── 5. Default Departments ──────────────────────────────────────────────────
    private async Task SeedDepartmentsAsync()
    {
        var existing = await _context.Departments.AnyAsync();
        if (!existing)
        {
            var depts = new[]
            {
                new Department { Id = Guid.NewGuid(), Name = "ICT", RoutingMode = "members", Description = "Information and Communication Technology department" },
                new Department { Id = Guid.NewGuid(), Name = "Resolution", RoutingMode = "members", Description = "Case resolution and mediation department" },
                new Department { Id = Guid.NewGuid(), Name = "Corporate HQ", RoutingMode = "members", Description = "Corporate Headquarters" }
            };
            await _context.Departments.AddRangeAsync(depts);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Seeded default departments");
        }
    }

    private async Task SeedPayGradesAsync()
    {
        if (!await _context.PayGrades.AnyAsync())
        {
            var grades = new List<PayGrade>
            {
                new PayGrade { Id = Guid.NewGuid(), Name = "Director Grade", Level = 16, BasicSalaryBand = "1,200,000 - 1,500,000", Currency = "NGN", MinSalary = 1200000, MaxSalary = 1500000, Description = "Director level grade band" },
                new PayGrade { Id = Guid.NewGuid(), Name = "Deputy Director Scale", Level = 15, BasicSalaryBand = "950,000 - 1,150,000", Currency = "NGN", MinSalary = 950000, MaxSalary = 1150000, Description = "Deputy Director level scale" },
                new PayGrade { Id = Guid.NewGuid(), Name = "Assistant Director Scale", Level = 14, BasicSalaryBand = "800,000 - 920,000", Currency = "NGN", MinSalary = 800000, MaxSalary = 920000, Description = "Assistant Director scale" },
                new PayGrade { Id = Guid.NewGuid(), Name = "Principal Officer Grade", Level = 12, BasicSalaryBand = "600,000 - 750,000", Currency = "NGN", MinSalary = 600000, MaxSalary = 750000, Description = "Principal Officer Grade" },
                new PayGrade { Id = Guid.NewGuid(), Name = "Senior Officer Grade", Level = 10, BasicSalaryBand = "450,000 - 580,000", Currency = "NGN", MinSalary = 450000, MaxSalary = 580000, Description = "Senior Officer Grade" },
                new PayGrade { Id = Guid.NewGuid(), Name = "Officer Grade I", Level = 9, BasicSalaryBand = "350,000 - 430,000", Currency = "NGN", MinSalary = 350000, MaxSalary = 430000, Description = "Officer Grade I" },
                new PayGrade { Id = Guid.NewGuid(), Name = "Officer Grade II", Level = 8, BasicSalaryBand = "280,000 - 340,000", Currency = "NGN", MinSalary = 280000, MaxSalary = 340000, Description = "Officer Grade II and probation scale." },
                new PayGrade { Id = Guid.NewGuid(), Name = "Assistant Officer", Level = 7, BasicSalaryBand = "200,000 - 260,000", Currency = "NGN", MinSalary = 200000, MaxSalary = 260000, Description = "Assistant Officer Entry Grade" }
            };
            await _context.PayGrades.AddRangeAsync(grades);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Seeded default pay grades");
        }
    }

    private async Task SeedPerformanceSettingsAsync()
    {
        if (!await _context.Competencies.AnyAsync())
        {
            var competencies = new List<Competency>
            {
                new Competency { Id = Guid.NewGuid(), Name = "Technical Competence", Description = "Demonstrated knowledge and skill in area of specialty.", SortOrder = 1, Status = "Active" },
                new Competency { Id = Guid.NewGuid(), Name = "Leadership & Supervision", Description = "Guidance, mentorship, and effective resource management.", SortOrder = 2, Status = "Active" },
                new Competency { Id = Guid.NewGuid(), Name = "Communication Skills", Description = "Clarity of expression, reporting, and team collaboration.", SortOrder = 3, Status = "Active" },
                new Competency { Id = Guid.NewGuid(), Name = "Problem Solving & Innovation", Description = "Critical thinking, troubleshooting, and positive changes.", SortOrder = 4, Status = "Active" },
                new Competency { Id = Guid.NewGuid(), Name = "Professional Ethics & integrity", Description = "Commitment to public trust, confidentiality and codes.", SortOrder = 5, Status = "Active" }
            };
            await _context.Competencies.AddRangeAsync(competencies);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Seeded default competencies");
        }

        if (!await _context.ReviewTemplates.AnyAsync())
        {
            var templates = new List<ReviewTemplate>
            {
                new ReviewTemplate { Id = Guid.NewGuid(), Name = "Annual Performance Review Template", Description = "Standard comprehensive appraisal template for annual evaluation cycles.", QuestionCount = 10, Status = "Active" },
                new ReviewTemplate { Id = Guid.NewGuid(), Name = "Mid-Year Progress Assessment", Description = "Lightweight appraisal template for tracking OKR/goal alignment.", QuestionCount = 5, Status = "Active" },
                new ReviewTemplate { Id = Guid.NewGuid(), Name = "Probation Review Form", Description = "Appraisal template for new hires under review.", QuestionCount = 6, Status = "Active" }
            };
            await _context.ReviewTemplates.AddRangeAsync(templates);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Seeded default review templates");
        }
    }

    private async Task SeedPayrollAsync()
    {
        // 1. Payout Providers
        if (!await _context.PayoutProviders.AnyAsync())
        {
            var providers = new List<PayoutProvider>
            {
                new PayoutProvider { Id = Guid.NewGuid(), Name = "Flutterwave", Adapter = "flutterwave", Country = "NG", Currency = "NGN", Status = "Inactive", Notes = "Integrated for automated local transfers.", ProviderCode = "FLW" },
                new PayoutProvider { Id = Guid.NewGuid(), Name = "Paystack", Adapter = "paystack", Country = "NG", Currency = "NGN", Status = "Active", Notes = "Primary payment provider for retail staff payroll.", ProviderCode = "PSTK" },
                new PayoutProvider { Id = Guid.NewGuid(), Name = "Manual Bank Transfer (NGN)", Adapter = "manual", Country = "NG", Currency = "NGN", Status = "Active", Notes = "Supports offline posting and bank scheduler downloads.", ProviderCode = "MAN_NGN" },
                new PayoutProvider { Id = Guid.NewGuid(), Name = "Manual Bank Transfer (USD)", Adapter = "manual", Country = "US", Currency = "USD", Status = "Active", Notes = "Supports offshore wire uploads.", ProviderCode = "MAN_USD" }
            };
            await _context.PayoutProviders.AddRangeAsync(providers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Payout providers seeded");
        }

        // 2. Statutory Deductions & Rules
        if (!await _context.StatutoryDeductions.AnyAsync())
        {
            var pensionEe = new StatutoryDeduction { Id = Guid.NewGuid(), Name = "Employee Pension (RSA)", Code = "PENSION_EE", Country = "NG", IsEmployee = true, IsEmployer = false, Status = "Active" };
            var pensionEr = new StatutoryDeduction { Id = Guid.NewGuid(), Name = "Employer Pension (RSA)", Code = "PENSION_ER", Country = "NG", IsEmployee = false, IsEmployer = true, Status = "Active" };
            var nhf = new StatutoryDeduction { Id = Guid.NewGuid(), Name = "National Housing Fund (NHF)", Code = "NHF", Country = "NG", IsEmployee = true, IsEmployer = false, Status = "Active" };
            var nsitf = new StatutoryDeduction { Id = Guid.NewGuid(), Name = "NSITF Contribution", Code = "NSITF", Country = "NG", IsEmployee = false, IsEmployer = true, Status = "Active" };
            var paye = new StatutoryDeduction { Id = Guid.NewGuid(), Name = "PAYE Tax (LIRS)", Code = "PAYE", Country = "NG", IsEmployee = true, IsEmployer = false, Status = "Active" };

            await _context.StatutoryDeductions.AddRangeAsync(pensionEe, pensionEr, nhf, nsitf, paye);
            await _context.SaveChangesAsync();

            var rules = new List<StatutoryRule>
            {
                new StatutoryRule { Id = Guid.NewGuid(), DeductionId = pensionEe.Id, AppliesTo = "All Employees", Basis = "Basic + Housing + Transport", RateOrAmount = 8.00m, RateOrAmountStr = "8.00%", EffectiveDate = new DateTime(2026, 1, 1), IsActive = true },
                new StatutoryRule { Id = Guid.NewGuid(), DeductionId = pensionEr.Id, AppliesTo = "All Employees", Basis = "Basic + Housing + Transport", RateOrAmount = 10.00m, RateOrAmountStr = "10.00%", EffectiveDate = new DateTime(2026, 1, 1), IsActive = true },
                new StatutoryRule { Id = Guid.NewGuid(), DeductionId = nhf.Id, AppliesTo = "Employees earning > NGN 3,000 monthly", Basis = "Basic Salary Only", RateOrAmount = 2.50m, RateOrAmountStr = "2.50%", EffectiveDate = new DateTime(2026, 1, 1), IsActive = true },
                new StatutoryRule { Id = Guid.NewGuid(), DeductionId = nsitf.Id, AppliesTo = "All Employees", Basis = "Gross Salary", RateOrAmount = 1.00m, RateOrAmountStr = "1.00%", EffectiveDate = new DateTime(2026, 1, 1), IsActive = true },
                new StatutoryRule { Id = Guid.NewGuid(), DeductionId = paye.Id, AppliesTo = "All Taxable Staff", Basis = "Taxable Income (Gross - Reliefs)", RateOrAmount = 0.00m, RateOrAmountStr = "Graduated Scale (7% - 24%)", EffectiveDate = new DateTime(2026, 1, 1), IsActive = true }
            };
            await _context.StatutoryRules.AddRangeAsync(rules);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Statutory deductions & rules seeded");
        }

        // 3. Payroll Periods
        if (!await _context.PayrollPeriods.AnyAsync())
        {
            var periods = new List<PayrollPeriod>
            {
                new PayrollPeriod { Id = Guid.NewGuid(), Name = "May 2026 Payroll", StartDate = new DateTimeOffset(new DateTime(2026, 5, 1)), EndDate = new DateTimeOffset(new DateTime(2026, 5, 31)), Currency = "NGN", Status = "open" },
                new PayrollPeriod { Id = Guid.NewGuid(), Name = "April 2026 Payroll", StartDate = new DateTimeOffset(new DateTime(2026, 4, 1)), EndDate = new DateTimeOffset(new DateTime(2026, 4, 30)), Currency = "NGN", Status = "closed" },
                new PayrollPeriod { Id = Guid.NewGuid(), Name = "March 2026 Payroll", StartDate = new DateTimeOffset(new DateTime(2026, 3, 1)), EndDate = new DateTimeOffset(new DateTime(2026, 3, 31)), Currency = "NGN", Status = "closed" }
            };
            await _context.PayrollPeriods.AddRangeAsync(periods);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Payroll periods seeded");
        }

        // 4. Salary Profiles
        var users = await _userManager.Users.ToListAsync();
        var componentsJson = @"[
            {""name"": ""Basic Salary"", ""category"": ""earning"", ""calculationType"": ""percentage"", ""value"": 50},
            {""name"": ""Housing Allowance"", ""category"": ""allowance"", ""calculationType"": ""percentage"", ""value"": 20},
            {""name"": ""Transport Allowance"", ""category"": ""allowance"", ""calculationType"": ""percentage"", ""value"": 10},
            {""name"": ""Pension Contribution"", ""category"": ""deduction"", ""calculationType"": ""percentage"", ""value"": 8},
            {""name"": ""PAYE Tax"", ""category"": ""deduction"", ""calculationType"": ""percentage"", ""value"": 10}
        ]";
        foreach (var u in users)
        {
            if (!await _context.SalaryProfiles.AnyAsync(sp => sp.UserId == u.Id))
            {
                var baseSal = u.Email == "admin@taxombud.gov.ng" ? 450000.00m : 280000.00m;
                var sp = new SalaryProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = u.Id,
                    Basic = baseSal,
                    EffectiveFrom = DateTimeOffset.UtcNow,
                    Allowances = componentsJson,
                    Deductions = "[]",
                    Currency = "NGN",
                    Status = "Active"
                };
                await _context.SalaryProfiles.AddAsync(sp);
            }
        }
        await _context.SaveChangesAsync();

        // 5. Payroll Runs & Entries (one posted run per closed period, one entry per staff user)
        if (!await _context.PayrollRuns.AnyAsync())
        {
            var closedPeriods = await _context.PayrollPeriods
                .Where(p => p.Status == "closed")
                .ToListAsync();

            var staffUsers2 = await _userManager.Users
                .Where(u => u.UserType == UserType.StaffUser)
                .ToListAsync();

            foreach (var period in closedPeriods)
            {
                var run = new PayrollRun
                {
                    Id = Guid.NewGuid(),
                    PeriodId = period.Id,
                    RunType = "regular",
                    Status = "posted",
                    Currency = "NGN",
                    EmployeesCount = staffUsers2.Count,
                    PostedAt = period.EndDate.AddDays(5),
                    ApprovedAt = period.EndDate.AddDays(3)
                };
                await _context.PayrollRuns.AddAsync(run);
                await _context.SaveChangesAsync();

                foreach (var u2 in staffUsers2)
                {
                    var basic = u2.Email == "admin@taxombud.gov.ng" ? 450_000m : 280_000m;
                    var housing  = Math.Round(basic * 0.20m, 2);
                    var transport = Math.Round(basic * 0.10m, 2);
                    var allowances = housing + transport;
                    var gross    = basic + allowances;
                    var pension  = Math.Round(gross * 0.08m, 2);
                    var nhf      = Math.Round(basic * 0.025m, 2);
                    var paye     = Math.Round(gross * 0.10m, 2);
                    var totalDeductions = pension + nhf + paye;
                    var net      = gross - totalDeductions;

                    var entry = new PayrollEntry
                    {
                        Id = Guid.NewGuid(),
                        RunId = run.Id,
                        UserId = u2.Id,
                        Basic = basic,
                        Allowances = allowances,
                        Gross = gross,
                        Paye = paye,
                        Pension = pension,
                        Nhf = nhf,
                        OtherStatutory = 0,
                        Deductions = totalDeductions,
                        Net = net,
                        PaymentStatus = "paid"
                    };
                    await _context.PayrollEntries.AddAsync(entry);
                }
                run.TotalGross = staffUsers2.Count * 280_000m;
                run.TotalNet   = staffUsers2.Count * 280_000m;
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation("✓ Payroll runs & payslip entries seeded");
        }
    }

    private async Task SeedStaffProfilesAsync()
    {
        var staffUsers = await _userManager.Users.Where(u => u.UserType == UserType.StaffUser).ToListAsync();
        foreach (var user in staffUsers)
        {
            var exists = await _context.StaffProfiles.AnyAsync(sp => sp.UserId == user.Id);
            StaffProfile profileObj;
            if (!exists)
            {
                profileObj = new StaffProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EmployeeCode = "EMP-" + user.FirstName.Substring(0, Math.Min(3, user.FirstName.Length)).ToUpper() + user.LastName.Substring(0, Math.Min(1, user.LastName.Length)).ToUpper(),
                    Title = "Mr",
                    HireDate = DateTimeOffset.UtcNow.AddYears(-1),
                    DateOfBirth = new DateTime(1995, 1, 1),
                    EmploymentStatus = "Active",
                    Nationality = "Nigerian",
                    MaritalStatus = "Single",
                    BankAccountNo = "0123456789",
                    BankId = "044" // Access Bank
                };
                await _context.StaffProfiles.AddAsync(profileObj);
                await _context.SaveChangesAsync();
            }
            else
            {
                profileObj = await _context.StaffProfiles.FirstAsync(sp => sp.UserId == user.Id);
            }

            // Seed Wallets & Transactions
            var hasWallet = await _context.EmployeeWallets.AnyAsync(w => w.UserId == user.Id);
            if (!hasWallet)
            {
                var wallet = new EmployeeWallet
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    BalanceNgn = user.Email == "admin@taxombud.gov.ng" ? 461750.00m : 15450.00m,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };
                await _context.EmployeeWallets.AddAsync(wallet);
                await _context.SaveChangesAsync();

                await _context.WalletTransactions.AddRangeAsync(
                    new WalletTransaction { Id = Guid.NewGuid(), WalletId = wallet.Id, Type = "credit", Amount = 650000m, Reference = "Salary Credit", Status = "paid", CreatedAt = DateTime.UtcNow.AddDays(-10) },
                    new WalletTransaction { Id = Guid.NewGuid(), WalletId = wallet.Id, Type = "debit", Amount = -20000m, Reference = "WithdrawalRequest", Status = "approved", CreatedAt = DateTime.UtcNow.AddDays(-8) },
                    new WalletTransaction { Id = Guid.NewGuid(), WalletId = wallet.Id, Type = "credit", Amount = 45000m, Reference = "EWA Payout", Status = "paid", CreatedAt = DateTime.UtcNow.AddDays(-7) }
                );
                await _context.SaveChangesAsync();
            }

            // Seed Loan/Advance requests
            var hasSalaryAdvance = await _context.LoanRequests.AnyAsync(l => l.UserId == user.Id && l.IsSalaryAdvance);
            if (!hasSalaryAdvance)
            {
                await _context.LoanRequests.AddAsync(new LoanRequest
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Amount = 80000m,
                    TermMonths = 1,
                    Purpose = "Salary Advance",
                    Status = "approved",
                    IsSalaryAdvance = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                });
            }

            var hasLoan = await _context.LoanRequests.AnyAsync(l => l.UserId == user.Id && !l.IsSalaryAdvance);
            if (!hasLoan)
            {
                await _context.LoanRequests.AddAsync(new LoanRequest
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Amount = 500000m,
                    TermMonths = 12,
                    Purpose = "Asset Purchase",
                    Status = "disbursed",
                    IsSalaryAdvance = false,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                });
            }

            // Seed EwaRequest
            var hasEwa = await _context.EwaRequests.AnyAsync(e => e.UserId == user.Id);
            if (!hasEwa)
            {
                await _context.EwaRequests.AddAsync(new EwaRequest
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Amount = 45000m,
                    Status = "approved",
                    DisbursedAt = DateTime.UtcNow.AddDays(-7),
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                });
            }

            // Seed StaffDocuments
            var docCount = await _context.Set<StaffDocument>().CountAsync(d => d.StaffProfileId == profileObj.Id);
            if (docCount == 0)
            {
                await _context.Set<StaffDocument>().AddRangeAsync(
                    new StaffDocument { Id = Guid.NewGuid(), StaffProfileId = profileObj.Id, FileName = "Employment Offer Letter", FileUrl = "/documents/offer_letter.pdf", DocumentType = "Contract", CreatedAt = DateTime.UtcNow.AddYears(-1) },
                    new StaffDocument { Id = Guid.NewGuid(), StaffProfileId = profileObj.Id, FileName = "NDPR Privacy Consent Form", FileUrl = "/documents/privacy_consent.pdf", DocumentType = "Compliance", CreatedAt = DateTime.UtcNow.AddYears(-1) },
                    new StaffDocument { Id = Guid.NewGuid(), StaffProfileId = profileObj.Id, FileName = "National ID Card Copy", FileUrl = "/documents/national_id.pdf", DocumentType = "Identification", CreatedAt = DateTime.UtcNow.AddMonths(-6) }
                );
            }
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Staff profiles, documents and wallet transactions seeded");
    }

    private async Task SeedPeopleOpsAsync()
    {
        // 1. Benefit Plans (BenefitTypes)
        if (!await _context.BenefitTypes.AnyAsync())
        {
            var hmo = new BenefitType { Id = Guid.NewGuid(), Name = "Group Health Insurance (HMO)", Code = "HMO_LEADWAY", Category = "Health", AffectsPayroll = true, IsTaxable = false, IsActive = true };
            var life = new BenefitType { Id = Guid.NewGuid(), Name = "Life Insurance", Code = "LIFE_AXA", Category = "Financial", AffectsPayroll = false, IsTaxable = false, IsActive = true };
            var gym = new BenefitType { Id = Guid.NewGuid(), Name = "Gym & Wellness Allowance", Code = "GYM_ALL", Category = "Wellness", AffectsPayroll = true, IsTaxable = true, IsActive = true };
            var cert = new BenefitType { Id = Guid.NewGuid(), Name = "Professional Certification Fund", Code = "CERT_FUND", Category = "Professional", AffectsPayroll = false, IsTaxable = false, IsActive = true };
            var dental = new BenefitType { Id = Guid.NewGuid(), Name = "Dental & Optical Cover", Code = "DENTAL_HYGEIA", Category = "Health", AffectsPayroll = false, IsTaxable = false, IsActive = true };

            await _context.BenefitTypes.AddRangeAsync(hmo, life, gym, cert, dental);
            await _context.SaveChangesAsync();

            // Enroll a couple of employees
            var staff = await _userManager.Users.Where(u => u.UserType == UserType.StaffUser).ToListAsync();
            if (staff.Any())
            {
                var enr1 = new EmployeeBenefit { Id = Guid.NewGuid(), EmployeeId = staff[0].Id, BenefitTypeId = hmo.Id, StartDate = DateTime.UtcNow.AddMonths(-6), Status = "Active" };
                var enr2 = new EmployeeBenefit { Id = Guid.NewGuid(), EmployeeId = staff[0].Id, BenefitTypeId = gym.Id, StartDate = DateTime.UtcNow.AddMonths(-3), Status = "Active" };
                
                if (staff.Count > 1)
                {
                    var enr3 = new EmployeeBenefit { Id = Guid.NewGuid(), EmployeeId = staff[1].Id, BenefitTypeId = life.Id, StartDate = DateTime.UtcNow.AddMonths(-6), Status = "Active" };
                    await _context.EmployeeBenefits.AddRangeAsync(enr1, enr2, enr3);
                }
                else
                {
                    await _context.EmployeeBenefits.AddRangeAsync(enr1, enr2);
                }
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation("✓ Benefits & enrollments seeded");
        }

        // 2. Performance Cycles
        if (!await _context.PerformanceCycles.AnyAsync())
        {
            var h1 = new PerformanceCycle { Id = Guid.NewGuid(), Name = "H1 2026 Mid-Year Performance Evaluation", StartDate = DateTime.UtcNow.AddMonths(-1), EndDate = DateTime.UtcNow.AddDays(15), Status = "Active" };
            var q3 = new PerformanceCycle { Id = Guid.NewGuid(), Name = "Q3 2026 Mid-Level Probation Review", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), Status = "Draft" };

            await _context.PerformanceCycles.AddRangeAsync(h1, q3);
            await _context.SaveChangesAsync();
        }

        var activeCycle = await _context.PerformanceCycles.FirstOrDefaultAsync(c => c.Status == "Active");
        if (activeCycle != null)
        {
            var staff = await _userManager.Users.Where(u => u.UserType == UserType.StaffUser).ToListAsync();
            var adminUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == "admin@taxombud.gov.ng");
            var reviewerId = adminUser?.Id ?? (staff.Any() ? staff[0].Id : Guid.Empty);

            foreach (var s in staff)
            {
                var profile = await _context.StaffProfiles.FirstOrDefaultAsync(p => p.UserId == s.Id);
                var targetId = profile?.Id ?? s.Id;

                var hasGoal = await _context.PerformanceGoals.AnyAsync(g => g.EmployeeId == s.Id || g.EmployeeId == targetId);
                if (!hasGoal)
                {
                    await _context.PerformanceGoals.AddAsync(new PerformanceGoal
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = targetId,
                        CycleId = activeCycle.Id,
                        Title = "Deliver onboarding training for new operations batch",
                        Description = "Deliver training to 15 new hires.",
                        ProgressPercentage = 10,
                        Status = "Active"
                    });
                }

                var hasReview = await _context.PerformanceReviews.AnyAsync(r => r.EmployeeId == s.Id || r.EmployeeId == targetId);
                if (!hasReview)
                {
                    await _context.PerformanceReviews.AddAsync(new PerformanceReview
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = targetId,
                        ReviewerId = reviewerId,
                        CycleId = activeCycle.Id,
                        Score = 5.0m,
                        ReviewerNotes = "Exceptional results, highly dedicated.",
                        Status = "Completed"
                    });
                }
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Performance cycles, goals & reviews seeded");
        }

        // 3. Disciplinary Cases
        if (!await _context.DisciplinaryCases.AnyAsync())
        {
            var staff = await _userManager.Users.Where(u => u.UserType == UserType.StaffUser).ToListAsync();
            if (staff.Any())
            {
                var case1 = new DisciplinaryCase
                {
                    Id = Guid.NewGuid(),
                    CaseReference = "DC-9284",
                    EmployeeId = staff[0].Id,
                    HrOfficerId = Guid.Empty,
                    IncidentType = "Workplace Conduct",
                    IncidentDate = DateTime.UtcNow.AddMonths(-2),
                    HearingDate = DateTime.UtcNow.AddMonths(-2).AddDays(10),
                    Description = "Insubordination - refused direct supervisor instruction.",
                    ActionTaken = "Written Warning Issued. First written warning.",
                    Outcome = "Written Warning Issued",
                    Status = "Resolved",
                    IsConfidential = false
                };

                var case2 = new DisciplinaryCase
                {
                    Id = Guid.NewGuid(),
                    CaseReference = "DC-1094",
                    EmployeeId = staff[0].Id,
                    HrOfficerId = Guid.Empty,
                    IncidentType = "Attendance",
                    IncidentDate = DateTime.UtcNow.AddDays(-5),
                    Description = "Repeated lateness to work (7 instances in 2 months).",
                    Status = "Open",
                    IsConfidential = false
                };

                await _context.DisciplinaryCases.AddRangeAsync(case1, case2);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✓ Disciplinary cases seeded");
            }
        }

        // 4. Exit Records
        if (!await _context.ExitRecords.AnyAsync())
        {
            var staff = await _userManager.Users.Where(u => u.UserType == UserType.StaffUser).ToListAsync();
            if (staff.Count > 1)
            {
                var exit = new ExitRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = staff[1].Id,
                    ExitType = "Resignation",
                    NoticeDate = DateTime.UtcNow.AddDays(-10),
                    LastWorkingDate = DateTime.UtcNow.AddDays(20),
                    Reason = "Resigned to take up a position at a law firm.",
                    Status = "Approved"
                };

                await _context.ExitRecords.AddAsync(exit);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✓ Exit records seeded");
            }
        }
    }

    private async Task SeedAccountsAsync()
    {
        if (!await _context.Accounts.AnyAsync())
        {
            var initialAccounts = new List<Account>
            {
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "South West",
                    HealthScore = 55,
                    Phone = "+2348000000005",
                    AltPhone = "",
                    Email = "contactus@taxombud.gov.ng",
                    Website = "https://example.com",
                    Country = "Nigeria",
                    Status = "Active",
                    Description = "South West regional office managing Lagos, Oyo, Ogun, Ondo, Osun, and Ekiti zones.",
                    Address = "12, Awolowo Road, Ikoyi, Lagos",
                    State = "Lagos",
                    City = "Ikoyi",
                    PostalCode = "101233",
                    Industry = "Tax Ombud",
                    IsWorkflowLane = true
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "South South",
                    HealthScore = 55,
                    Phone = "+2348000000004",
                    AltPhone = "",
                    Email = "contactus@taxombud.gov.ng",
                    Website = "https://example.com",
                    Country = "Nigeria",
                    Status = "Active",
                    Description = "South South regional office managing Rivers, Delta, Edo, Akwa Ibom, Cross River, and Bayelsa zones.",
                    Address = "45, Port Harcourt Road, Port Harcourt",
                    State = "Rivers",
                    City = "Port Harcourt",
                    PostalCode = "500272",
                    Industry = "Tax Ombud",
                    IsWorkflowLane = true
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "South East",
                    HealthScore = 55,
                    Phone = "+2348000000003",
                    AltPhone = "",
                    Email = "contactus@taxombud.gov.ng",
                    Website = "https://example.com",
                    Country = "Nigeria",
                    Status = "Active",
                    Description = "South East regional office managing Enugu, Anambra, Abia, Imo, and Ebonyi zones.",
                    Address = "8, Okpara Avenue, Enugu",
                    State = "Enugu",
                    City = "Enugu",
                    PostalCode = "400102",
                    Industry = "Tax Ombud",
                    IsWorkflowLane = true
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "North West",
                    HealthScore = 55,
                    Phone = "+2348000000002",
                    AltPhone = "",
                    Email = "contactus@taxombud.gov.ng",
                    Website = "https://example.com",
                    Country = "Nigeria",
                    Status = "Active",
                    Description = "North West regional office managing Kaduna, Kano, Katsina, Jigawa, Kebbi, Sokoto, and Zamfara zones.",
                    Address = "22, Isa Kaita Road, Kaduna",
                    State = "Kaduna",
                    City = "Kaduna",
                    PostalCode = "800283",
                    Industry = "Tax Ombud",
                    IsWorkflowLane = true
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "North East",
                    HealthScore = 55,
                    Phone = "+2348000000001",
                    AltPhone = "",
                    Email = "contactus@taxombud.gov.ng",
                    Website = "https://example.com",
                    Country = "Nigeria",
                    Status = "Active",
                    Description = "North East regional office managing Borno, Yobe, Adamawa, Taraba, Bauchi, and Gombe zones.",
                    Address = "15, Maiduguri Bypass, Bauchi",
                    State = "Bauchi",
                    City = "Bauchi",
                    PostalCode = "740211",
                    Industry = "Tax Ombud",
                    IsWorkflowLane = true
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "North Central",
                    HealthScore = 70,
                    Phone = "+2348000000000",
                    AltPhone = "",
                    Email = "contactus@taxombud.gov.ng",
                    Website = "https://example.com",
                    Country = "Nigeria",
                    Status = "Active",
                    Description = "North Central regional office managing FCT, Plateau, Nasarawa, Niger, Benue, Kogi, and Kwara zones.",
                    Address = "Plot 1024, Constitution Avenue, Central Business District, Abuja",
                    State = "Federal Capital Territory",
                    City = "Abuja",
                    PostalCode = "900211",
                    Industry = "Tax Ombud",
                    IsWorkflowLane = true
                }
            };
            await _context.Accounts.AddRangeAsync(initialAccounts);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Seeded 6 default zonal accounts");
        }
    }

    private async Task SeedTaxpayersAsync()
    {
        var dummyEmails = new[]
        {
            "michelojuade@gmail.com",
            "goldbitagency@gmail.com",
            "azareljaja@gmail.com",
            "danielnoseh@gmail.com",
            "habeebafolabi92@gmail.com",
            "eigbe.alright@nigerianbar.ng",
            "eigbe.alright@nigerianbar.org",
            "alrightspassion@gmail.com",
            "kelvin@ugbana.com"
        };

        foreach (var email in dummyEmails)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var profile = await _context.TaxpayerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (profile != null)
                {
                    _context.TaxpayerProfiles.Remove(profile);
                }
                await _userManager.DeleteAsync(user);
            }
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Cleared dummy taxpayers from database");
    }

    private async Task SeedKnowledgeCenterAsync()
    {
        if (await _context.KnowledgeCategories.AnyAsync())
        {
            return;
        }

        var cats = new List<TaxOmbud.Domain.Entities.Knowledge.KnowledgeCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Complaints Filing Process", Slug = "complaints-filing", Description = "Guidelines on how to lodge a tax dispute, upload documents, and track feedback.", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "VAT Waiver Regulations", Slug = "vat-waiver", Description = "Legal guidelines and requirements regarding late VAT filing penalty waivers.", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "SLA Escalation Protocols", Slug = "sla-protocols", Description = "Internal SLA standards, dispute reassignment turnaround windows, and timelines.", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Portal Trouble Shooting", Slug = "troubleshooting", Description = "Fixing login errors, browser session timeouts, and token authentication failures.", CreatedAt = DateTime.UtcNow }
        };

        await _context.KnowledgeCategories.AddRangeAsync(cats);
        await _context.SaveChangesAsync();

        var topics = new List<TaxOmbud.Domain.Entities.Knowledge.KnowledgeTopic>
        {
            new() {
                Id = Guid.NewGuid(),
                CategoryId = cats[0].Id,
                Title = "How to file a new complaint",
                Body = "To file a complaint, navigate to the Complaints tab and click \"New Complaint\". Ensure you include the FIRS Assessment reference.",
                TagsJson = "[\"filing\",\"complaints\"]",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                CategoryId = cats[0].Id,
                Title = "Uploading support documents",
                Body = "Upload PDF or JPG documents under the \"Attachments\" tab of the case record. File size is capped at 15MB.",
                TagsJson = "[\"upload\",\"attachments\"]",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                CategoryId = cats[1].Id,
                Title = "Section 32 Waiver Guidelines",
                Body = "VAT Act Section 32 allows waiver requests if taxpayer proves portal outages on the final day. Access logs must be attached.",
                TagsJson = "[\"vat\",\"waiver\",\"firs\"]",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                CategoryId = cats[2].Id,
                Title = "Intake officer response limits",
                Body = "The Intake & Verification officer has 48 hours to confirm reception and validity of complaints before escalating to Assessment.",
                TagsJson = "[\"sla\",\"intake\"]",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                CategoryId = cats[3].Id,
                Title = "Clear portal browser cache",
                Body = "If log-in session fails, clear browser cache or run in incognito mode. Session timeouts default to 30 minutes.",
                TagsJson = "[\"portal\",\"cache\"]",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.KnowledgeTopics.AddRangeAsync(topics);
        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Seeded default Knowledge Center categories and topics");
    }

    private async Task SeedChatsAsync()
    {
        if (await _context.AgentChats.AnyAsync())
        {
            return;
        }

        var allUsers = await _userManager.Users.ToListAsync();
        if (!allUsers.Any()) return;

        var participantIds = allUsers.Select(u => u.Id.ToString()).ToList();
        
        var generalChat = new TaxOmbud.Domain.Entities.Communications.AgentChat
        {
            Id = Guid.NewGuid(),
            Topic = "General Staff Lounge",
            IsGroupChat = true,
            ParticipantIds = System.Text.Json.JsonSerializer.Serialize(participantIds),
            CreatedAt = DateTime.UtcNow
        };

        await _context.AgentChats.AddAsync(generalChat);
        await _context.SaveChangesAsync();

        var systemAdmin = allUsers.FirstOrDefault(u => u.Email == "admin@taxombud.gov.ng");
        if (systemAdmin != null)
        {
            var welcomeMessage = new TaxOmbud.Domain.Entities.Communications.AgentChatMessage
            {
                Id = Guid.NewGuid(),
                AgentChatId = generalChat.Id,
                SenderId = systemAdmin.Id,
                Content = "Welcome to the Tax Ombud staff lounge! Use this channel for secure, team-wide announcements and direct chats.",
                ReadReceipts = "[]",
                CreatedAt = DateTime.UtcNow
            };

            await _context.AgentChatMessages.AddAsync(welcomeMessage);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("✓ Seeded default General Staff Lounge chat thread and message");
    }

    private async Task SeedSystemSettingsAsync()
    {
        var settings = new[]
        {
            new { Key = "Smtp:Host", Value = "mail.ksmlagosmetro.com.ng", Description = "SMTP Server Hostname" },
            new { Key = "Smtp:Port", Value = "465", Description = "SMTP Server Port" },
            new { Key = "Smtp:UseSsl", Value = "true", Description = "SMTP Enable SSL/TLS (true/false)" },
            new { Key = "Smtp:Username", Value = "emailcheck@ksmlagosmetro.com.ng", Description = "SMTP Account Username" },
            new { Key = "Smtp:Password", Value = "Nanrotech@1", Description = "SMTP Account Password" },
            new { Key = "Smtp:FromAddress", Value = "emailcheck@ksmlagosmetro.com.ng", Description = "Sender Email Address" },
            new { Key = "Smtp:FromName", Value = "Tax Ombud System", Description = "Sender Display Name" },
            // E2EE is disabled — the frontend does not implement client-side encryption headers.
            // Enable only when a matching frontend E2EE implementation is in place.
            new { Key = "E2EE_ENABLED", Value = "false", Description = "End-to-End Encryption enabled status" },
            new { Key = "Security:E2EE_Enabled", Value = "false", Description = "Security layer E2EE flag" }
        };

        foreach (var s in settings)
        {
            if (!await _context.SystemSettings.AnyAsync(x => x.Key == s.Key))
            {
                await _context.SystemSettings.AddAsync(new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Key = s.Key,
                    Value = s.Value,
                    Description = s.Description
                });
            }
        }

        // ── Correction: if E2EE was previously seeded as "true", reset it to "false".
        // The frontend does not send E2EE headers, so enabling it blocks all requests.
        var e2eeKeys = new[] { "E2EE_ENABLED", "Security:E2EE_Enabled" };
        foreach (var key in e2eeKeys)
        {
            var existing = await _context.SystemSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (existing != null && existing.Value == "true")
            {
                existing.Value = "false";
                _logger.LogWarning("⚠ Reset {Key} from 'true' to 'false' — frontend does not support E2EE headers.", key);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Seeded default system/email settings");
    }

    private async Task SeedWorkflowsAsync()
    {
        if (!await _context.Workflows.AnyAsync())
        {
            var workflow = new TaxOmbud.Domain.Entities.Workflows.Workflow(
                "Standard Tax Ombud Case Resolution Workflow",
                "Default 4-level sequential case approval and resolution workflow",
                "General",
                isDefault: true
            );

            var officerRole = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Name == RoleConstants.Officer);
            var seniorRole = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Name == RoleConstants.SeniorOfficer);
            var managerRole = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Name == RoleConstants.Manager);
            var directorRole = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Name == RoleConstants.Director);

            var level1 = new TaxOmbud.Domain.Entities.Workflows.WorkflowLevel(
                workflow.Id, 1, "Level 1 - Intake & Verification", "Initial case verification and document check",
                Domain.Enums.AssignmentTargetType.Role, officerRole?.Id, null,
                Domain.Enums.AssignmentMode.Automatic, Domain.Enums.AssignmentAlgorithm.RoundRobin
            ) { SlaHours = 24, EscalationHours = 48, RequireComment = false };

            var level2 = new TaxOmbud.Domain.Entities.Workflows.WorkflowLevel(
                workflow.Id, 2, "Level 2 - Investigation & Finding", "Detailed tax dispute investigation and recommendation formulation",
                Domain.Enums.AssignmentTargetType.Role, seniorRole?.Id, null,
                Domain.Enums.AssignmentMode.Automatic, Domain.Enums.AssignmentAlgorithm.LeastWorkload
            ) { SlaHours = 48, EscalationHours = 72, RequireComment = true };

            var level3 = new TaxOmbud.Domain.Entities.Workflows.WorkflowLevel(
                workflow.Id, 3, "Level 3 - Supervisor Review", "Legal compliance and quality review of case recommendations",
                Domain.Enums.AssignmentTargetType.Role, managerRole?.Id, null,
                Domain.Enums.AssignmentMode.Automatic, Domain.Enums.AssignmentAlgorithm.RoundRobin
            ) { SlaHours = 48, EscalationHours = 72, RequireComment = true };

            var level4 = new TaxOmbud.Domain.Entities.Workflows.WorkflowLevel(
                workflow.Id, 4, "Level 4 - Executive Approval", "Final sign-off by Directorate Director / Ombud Executive",
                Domain.Enums.AssignmentTargetType.Role, directorRole?.Id, null,
                Domain.Enums.AssignmentMode.Automatic, Domain.Enums.AssignmentAlgorithm.FirstAvailable
            ) { SlaHours = 24, EscalationHours = 48, RequireComment = true };

            workflow.Levels.Add(level1);
            workflow.Levels.Add(level2);
            workflow.Levels.Add(level3);
            workflow.Levels.Add(level4);

            _context.Workflows.Add(workflow);
            await _context.SaveChangesAsync();

            // Create Version 1 snapshot
            var snapshotJson = System.Text.Json.JsonSerializer.Serialize(workflow, new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });

            var version = new TaxOmbud.Domain.Entities.Workflows.WorkflowVersion(workflow.Id, 1, snapshotJson);
            var superAdminUser = await _userManager.FindByEmailAsync("admin@taxombud.gov.ng");
            version.Publish(superAdminUser?.Id ?? Guid.Empty);

            _context.WorkflowVersions.Add(version);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✓ Seeded default 4-level Tax Ombud workflow template");
        }
    }
}
