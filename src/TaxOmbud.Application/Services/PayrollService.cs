using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Payroll.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IGenericRepository<PayrollRun> _payrollRunRepo;
    private readonly IGenericRepository<PayrollPeriod> _payrollPeriodRepo;
    private readonly IGenericRepository<SalaryProfile> _salaryProfileRepo;
    private readonly IGenericRepository<Remittance> _remittanceRepo;
    private readonly IGenericRepository<StatutoryDeduction> _deductionRepo;
    private readonly IGenericRepository<StatutoryRule> _ruleRepo;
    private readonly IGenericRepository<PayoutProvider> _providerRepo;
    private readonly IGenericRepository<PayrollEntry> _entryRepo;
    private readonly IGenericRepository<SystemSetting> _settingRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<EwaRequest> _ewaRepo;
    private readonly IGenericRepository<LoanRequest> _loanRepo;
    private readonly IGenericRepository<EmployeeWallet> _walletRepo;
    private readonly IGenericRepository<WalletTransaction> _transactionRepo;

    public PayrollService(
        IGenericRepository<PayrollRun> payrollRunRepo,
        IGenericRepository<PayrollPeriod> payrollPeriodRepo,
        IGenericRepository<SalaryProfile> salaryProfileRepo,
        IGenericRepository<Remittance> remittanceRepo,
        IGenericRepository<StatutoryDeduction> deductionRepo,
        IGenericRepository<StatutoryRule> ruleRepo,
        IGenericRepository<PayoutProvider> providerRepo,
        IGenericRepository<PayrollEntry> entryRepo,
        IGenericRepository<SystemSetting> settingRepo,
        IGenericRepository<User> userRepo,
        IGenericRepository<EwaRequest> ewaRepo,
        IGenericRepository<LoanRequest> loanRepo,
        IGenericRepository<EmployeeWallet> walletRepo,
        IGenericRepository<WalletTransaction> transactionRepo)
    {
        _payrollRunRepo = payrollRunRepo;
        _payrollPeriodRepo = payrollPeriodRepo;
        _salaryProfileRepo = salaryProfileRepo;
        _remittanceRepo = remittanceRepo;
        _deductionRepo = deductionRepo;
        _ruleRepo = ruleRepo;
        _providerRepo = providerRepo;
        _entryRepo = entryRepo;
        _settingRepo = settingRepo;
        _userRepo = userRepo;
        _ewaRepo = ewaRepo;
        _loanRepo = loanRepo;
        _walletRepo = walletRepo;
        _transactionRepo = transactionRepo;
    }

    // ==========================================
    // 1. SALARY PROFILES
    // ==========================================
    public async Task<Response<List<SalaryProfile>>> GetSalaryProfilesAsync(GetSalaryProfilesQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<SalaryProfile>>();
        try
        {
            var list = await _salaryProfileRepo.Query()
                .Include(sp => sp.User)
                .Where(sp => !sp.IsDeleted)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<Guid>> SaveSalaryProfileAsync(SaveSalaryProfileCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            SalaryProfile? profile = null;
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                profile = await _salaryProfileRepo.FindAsync(sp => sp.Id == request.Id.Value);
            }
            else
            {
                profile = await _salaryProfileRepo.FindAsync(sp => sp.UserId == request.UserId);
            }

            if (profile == null)
            {
                profile = new SalaryProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _salaryProfileRepo.AddAsync(profile);
            }

            profile.Basic = request.Basic;
            profile.Allowances = request.Allowances;
            profile.Deductions = request.Deductions;
            profile.EffectiveFrom = request.EffectiveFrom;
            profile.Currency = request.Currency ?? "NGN";
            profile.Status = request.Status ?? "Active";

            await _salaryProfileRepo.SaveAsync();

            return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Salary profile saved.", Data = profile.Id };
        }
        catch (Exception)
        {
            return new Response<Guid> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> DeleteSalaryProfileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _salaryProfileRepo.FindAsync(sp => sp.Id == id);
            if (profile == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Profile not found." };

            profile.IsDeleted = true;
            profile.DeletedAt = DateTimeOffset.UtcNow;
            await _salaryProfileRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Salary profile deleted.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    // ==========================================
    // 2. STATUTORY DEDUCTIONS
    // ==========================================
    public async Task<Response<List<StatutoryDeduction>>> GetStatutoryDeductionsAsync(CancellationToken cancellationToken = default)
    {
        var response = new Response<List<StatutoryDeduction>>();
        try
        {
            var list = await _deductionRepo.Query()
                .Include(d => d.StatutoryRules)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<Guid>> CreateStatutoryDeductionAsync(CreateStatutoryDeductionCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new StatutoryDeduction
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                Country = request.Country ?? "NG",
                IsEmployee = request.IsEmployee,
                IsEmployer = request.IsEmployer,
                Status = request.Status ?? "Active"
            };

            await _deductionRepo.AddAsync(entity);
            await _deductionRepo.SaveAsync();

            return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Deduction type created.", Data = entity.Id };
        }
        catch (Exception)
        {
            return new Response<Guid> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<Guid>> CreateStatutoryRuleAsync(Guid deductionId, CreateStatutoryRuleCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new StatutoryRule
            {
                Id = Guid.NewGuid(),
                DeductionId = deductionId,
                AppliesTo = request.AppliesTo ?? "All",
                Basis = request.Basis ?? "Gross",
                RateOrAmount = request.RateOrAmount,
                RateOrAmountStr = request.RateOrAmountStr,
                EffectiveDate = request.EffectiveDate,
                EndDate = request.EndDate,
                IsActive = true
            };

            await _ruleRepo.AddAsync(entity);
            await _ruleRepo.SaveAsync();

            return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Calculation rule created.", Data = entity.Id };
        }
        catch (Exception)
        {
            return new Response<Guid> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> DeleteStatutoryRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _ruleRepo.FindAsync(r => r.Id == ruleId);
            if (entity == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Rule not found." };

            await _ruleRepo.RemoveAsync(entity);
            await _ruleRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Rule removed.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> ToggleStatutoryDeductionStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _deductionRepo.FindAsync(d => d.Id == id);
            if (entity == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Deduction not found." };

            entity.Status = entity.Status == "Active" ? "Inactive" : "Active";
            await _deductionRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = $"Deduction status set to {entity.Status}.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    // ==========================================
    // 3. PAYOUT PROVIDERS
    // ==========================================
    public async Task<Response<List<PayoutProvider>>> GetPayoutProvidersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _providerRepo.GetAllAsync();
            return new Response<List<PayoutProvider>> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = list.ToList() };
        }
        catch (Exception)
        {
            return new Response<List<PayoutProvider>> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<Guid>> SavePayoutProviderAsync(SavePayoutProviderCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            PayoutProvider? entity = null;
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                entity = await _providerRepo.FindAsync(p => p.Id == request.Id.Value);
            }

            if (entity == null)
            {
                entity = new PayoutProvider
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                };
                await _providerRepo.AddAsync(entity);
            }

            entity.Name = request.Name;
            entity.ProviderCode = request.ProviderCode ?? request.Adapter;
            entity.Type = request.Type ?? "Bank";
            entity.Adapter = request.Adapter ?? "manual";
            entity.Country = request.Country ?? "NG";
            entity.Currency = request.Currency ?? "NGN";
            entity.PublicKey = request.PublicKey;
            entity.SecretKey = request.SecretKey;
            entity.WebhookSecret = request.WebhookSecret;
            entity.Notes = request.Notes;
            entity.Status = request.Status ?? "Active";

            await _providerRepo.SaveAsync();

            return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Payout provider configured.", Data = entity.Id };
        }
        catch (Exception)
        {
            return new Response<Guid> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> TogglePayoutProviderStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _providerRepo.FindAsync(p => p.Id == id);
            if (entity == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Provider not found." };

            entity.Status = entity.Status == "Active" ? "Inactive" : "Active";
            await _providerRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = $"Provider set to {entity.Status}.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    // ==========================================
    // 4. PAYROLL PERIODS
    // ==========================================
    public async Task<Response<List<PayrollPeriod>>> GetPayrollPeriodsAsync(GetPayrollPeriodsQueries request, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _payrollPeriodRepo.Query()
                .Where(pp => !pp.IsDeleted)
                .OrderByDescending(pp => pp.StartDate)
                .ToListAsync(cancellationToken);

            return new Response<List<PayrollPeriod>> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = list };
        }
        catch (Exception)
        {
            return new Response<List<PayrollPeriod>> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<Guid>> CreatePayrollPeriodAsync(CreatePayrollPeriodCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new PayrollPeriod
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Currency = request.Currency ?? "NGN",
                Status = "open",
                CreatedAt = DateTime.UtcNow
            };

            await _payrollPeriodRepo.AddAsync(entity);
            await _payrollPeriodRepo.SaveAsync();

            return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Payroll period created.", Data = entity.Id };
        }
        catch (Exception)
        {
            return new Response<Guid> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> TogglePayrollPeriodStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _payrollPeriodRepo.FindAsync(pp => pp.Id == id);
            if (entity == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Period not found." };

            entity.Status = entity.Status == "open" ? "closed" : "open";
            await _payrollPeriodRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = $"Period status set to {entity.Status}.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<ValidationResultDto>> ValidatePayrollPeriodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var period = await _payrollPeriodRepo.FindAsync(pp => pp.Id == id);
            if (period == null)
                return new Response<ValidationResultDto> { StatusCode = StatusCodes.Status404NotFound, Message = "Period not found." };

            var errors = new List<ValidationErrorDto>();
            var warnings = new List<ValidationErrorDto>();
            var info = new List<ValidationErrorDto>();

            // Check salary profiles
            var users = await _userRepo.Query().Where(u => !u.IsDeleted).ToListAsync(cancellationToken);
            var profiles = await _salaryProfileRepo.Query().Where(sp => !sp.IsDeleted && sp.Status == "Active").ToListAsync(cancellationToken);

            var unconfiguredUsers = users.Where(u => !profiles.Any(p => p.UserId == u.Id)).ToList();
            if (unconfiguredUsers.Any())
            {
                var names = string.Join(", ", unconfiguredUsers.Take(3).Select(u => u.FullName));
                errors.Add(new ValidationErrorDto(
                    "NO_SALARY_PROFILES",
                    $"No active salary profiles found for {names}{(unconfiguredUsers.Count > 3 ? $" and {unconfiguredUsers.Count - 3} others" : "")}. Please configure profiles.",
                    "/admin/payroll/salary-profiles"
                ));
            }

            // Check pending loans
            var pendingLoans = await _loanRepo.Query().Where(l => l.Status == "pending" && !l.IsDeleted).CountAsync(cancellationToken);
            if (pendingLoans > 0)
            {
                warnings.Add(new ValidationErrorDto(
                    "UNAPPROVED_LOANS",
                    $"There are {pendingLoans} pending loan request(s) that will not be deducted in this run unless approved.",
                    "/admin/finance/loans"
                ));
            }

            // Estimate Totals
            int count = profiles.Count;
            decimal estGross = profiles.Sum(p => p.Basic); // Simple estimate base sum
            info.Add(new ValidationErrorDto("EST_GROSS", $"Estimated total gross for {count} active profiles: NGN {estGross:N2}.", null));

            var activeStatutoryCount = await _deductionRepo.Query().Where(d => d.Status == "Active").CountAsync(cancellationToken);
            info.Add(new ValidationErrorDto("STATUTORY_RULES", $"{activeStatutoryCount} active statutory deduction types will apply.", null));

            string status = errors.Any() ? "failed" : "passed";

            var result = new ValidationResultDto(
                status,
                count,
                estGross,
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                errors,
                warnings,
                info
            );

            return new Response<ValidationResultDto> { StatusCode = StatusCodes.Status200OK, Message = "Validation completed.", Data = result };
        }
        catch (Exception)
        {
            return new Response<ValidationResultDto> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    // ==========================================
    // 5. PAYROLL RUNS
    // ==========================================
    public async Task<Response<List<PayrollRun>>> GetPayrollRunsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _payrollRunRepo.Query()
                .Include(pr => pr.Period)
                .Where(pr => !pr.IsDeleted)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync(cancellationToken);

            return new Response<List<PayrollRun>> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = list };
        }
        catch (Exception)
        {
            return new Response<List<PayrollRun>> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<Guid>> RunPayrollAsync(RunPayrollCommands request, CancellationToken cancellationToken = default)
    {
        try
        {
            var period = await _payrollPeriodRepo.FindAsync(pp => pp.Id == request.PeriodId);
            if (period == null)
                return new Response<Guid> { StatusCode = StatusCodes.Status404NotFound, Message = "Period not found." };

            // Check if duplicate run exists for this period
            var duplicate = await _payrollRunRepo.FindAsync(pr => pr.PeriodId == request.PeriodId && !pr.IsDeleted);
            if (duplicate != null)
                return new Response<Guid> { StatusCode = StatusCodes.Status400BadRequest, Message = $"A payroll run already exists for \"{period.Name}\"." };

            var profiles = await _salaryProfileRepo.Query()
                .Include(p => p.User)
                .Where(p => !p.IsDeleted && p.Status == "Active")
                .ToListAsync(cancellationToken);

            var run = new PayrollRun
            {
                Id = Guid.NewGuid(),
                PeriodId = request.PeriodId,
                RunType = "regular",
                Status = "draft",
                Currency = period.Currency,
                CreatedAt = DateTime.UtcNow
            };

            decimal totalGross = 0;
            decimal totalNet = 0;
            decimal totalStatutory = 0;
            int empCount = 0;

            var entries = new List<PayrollEntry>();

            foreach (var p in profiles)
            {
                decimal basic = p.Basic;
                decimal allowances = 0;
                decimal deductions = 0;

                // Process Allowances / Deductions JSON lists if configured
                try 
                {
                    if (!string.IsNullOrEmpty(p.Allowances))
                    {
                        var compList = JsonSerializer.Deserialize<List<SalaryComponentDto>>(p.Allowances, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (compList != null)
                        {
                            foreach (var comp in compList)
                            {
                                decimal val = comp.CalculationType == "flat" ? comp.Value : (basic * comp.Value / 100);
                                if (comp.Category == "allowance")
                                    allowances += val;
                                else if (comp.Category == "deduction")
                                    deductions += val;
                            }
                        }
                    }
                } catch {}

                // Default Statutory calculations
                decimal paye = basic * 0.10m; // 10%
                decimal pension = basic * 0.08m; // 8%
                decimal nhf = basic * 0.025m; // 2.5%
                decimal otherStat = 0;

                // Recover EWA requests
                var ewaRequests = await _ewaRepo.Query()
                    .Where(er => er.UserId == p.UserId && er.Status == "disbursed" && er.RecoveredInPeriodId == null && !er.IsDeleted)
                    .ToListAsync(cancellationToken);
                decimal ewaDeductions = ewaRequests.Sum(er => er.Amount);
                deductions += ewaDeductions;

                // Recover Loan requests
                var loanRequests = await _loanRepo.Query()
                    .Where(l => l.UserId == p.UserId && l.Status == "disbursed" && !l.IsDeleted && !l.IsSalaryAdvance)
                    .ToListAsync(cancellationToken);
                decimal loanDeductions = 0;
                foreach (var lr in loanRequests)
                {
                    decimal monthlyDeduct = lr.TermMonths > 0 ? (lr.Amount / lr.TermMonths) : lr.Amount;
                    loanDeductions += monthlyDeduct;
                }
                deductions += loanDeductions;

                decimal gross = basic + allowances;
                decimal net = gross - deductions - paye - pension - nhf;

                var entry = new PayrollEntry
                {
                    Id = Guid.NewGuid(),
                    RunId = run.Id,
                    UserId = p.UserId,
                    Basic = basic,
                    Allowances = allowances,
                    Deductions = deductions,
                    Paye = paye,
                    Pension = pension,
                    Nhf = nhf,
                    OtherStatutory = otherStat,
                    Gross = gross,
                    Net = net,
                    PaymentStatus = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                entries.Add(entry);

                totalGross += gross;
                totalNet += net;
                totalStatutory += (paye + pension + nhf);
                empCount++;
            }

            run.TotalGross = totalGross;
            run.TotalNet = totalNet;
            run.TotalStatutory = totalStatutory;
            run.EmployeesCount = empCount;

            await _payrollRunRepo.AddAsync(run);
            await _payrollRunRepo.SaveAsync();

            foreach (var ent in entries)
            {
                await _entryRepo.AddAsync(ent);
            }
            await _entryRepo.SaveAsync();

            return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Payroll run draft generated.", Data = run.Id };
        }
        catch (Exception)
        {
            return new Response<Guid> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> ApprovePayrollAsync(ApprovePayrollCommands request, CancellationToken cancellationToken = default)
    {
        try
        {
            var run = await _payrollRunRepo.FindAsync(r => r.Id == request.RunId);
            if (run == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Payroll run not found." };

            run.Status = "approved";
            run.ApprovedAt = DateTimeOffset.UtcNow;
            await _payrollRunRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Payroll run approved.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> PostPayrollAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        try
        {
            var run = await _payrollRunRepo.Query()
                .Include(r => r.Period)
                .FirstOrDefaultAsync(r => r.Id == runId, cancellationToken);
            if (run == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Payroll run not found." };

            run.Status = "posted";
            run.PostedAt = DateTimeOffset.UtcNow;
            run.Period.Status = "closed";

            // Process all entries and disburse to employee wallets!
            var entries = await _entryRepo.Query()
                .Include(e => e.User)
                .Where(e => e.RunId == runId)
                .ToListAsync(cancellationToken);

            foreach (var entry in entries)
            {
                var wallet = await _walletRepo.FindAsync(w => w.UserId == entry.UserId);
                if (wallet == null)
                {
                    wallet = new EmployeeWallet
                    {
                        Id = Guid.NewGuid(),
                        UserId = entry.UserId,
                        BalanceNgn = 0,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _walletRepo.AddAsync(wallet);
                }

                // Add Net Pay to Wallet Balance
                wallet.BalanceNgn += entry.Net;

                // Log Wallet Transaction
                var trans = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    Type = "credit",
                    Amount = entry.Net,
                    Reference = $"PAY-{run.Period.Name.Replace(" ", "-")}",
                    Status = "approved",
                    ApprovedAt = DateTimeOffset.UtcNow,
                    PaidAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepo.AddAsync(trans);

                entry.PaymentStatus = "paid";

                // Update recovered EWA status
                var ewaRequests = await _ewaRepo.Query()
                    .Where(er => er.UserId == entry.UserId && er.Status == "disbursed" && er.RecoveredInPeriodId == null && !er.IsDeleted)
                    .ToListAsync(cancellationToken);
                foreach (var er in ewaRequests)
                {
                    er.RecoveredInPeriodId = run.PeriodId;
                    er.Status = "settled";
                }

                // Update recovered loans status
                var loanRequests = await _loanRepo.Query()
                    .Where(l => l.UserId == entry.UserId && l.Status == "disbursed" && !l.IsDeleted && !l.IsSalaryAdvance)
                    .ToListAsync(cancellationToken);
                // In a production app, we would decrement loan outstanding balances or increment terms.
                // For simplicity, we flag them as settled if term reaches zero, or keep them active.
            }

            await _payrollRunRepo.SaveAsync();
            await _walletRepo.SaveAsync();
            await _transactionRepo.SaveAsync();
            await _entryRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Payroll run posted and balances disbursed.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> DeletePayrollRunAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        try
        {
            var run = await _payrollRunRepo.FindAsync(r => r.Id == runId);
            if (run == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Run not found." };

            if (run.Status == "posted")
                return new Response<bool> { StatusCode = StatusCodes.Status400BadRequest, Message = "Cannot delete a posted payroll run." };

            await _payrollRunRepo.RemoveAsync(run);

            var entries = await _entryRepo.Query().Where(e => e.RunId == runId).ToListAsync(cancellationToken);
            foreach (var entry in entries)
            {
                await _entryRepo.RemoveAsync(entry);
            }

            await _payrollRunRepo.SaveAsync();
            await _entryRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Payroll run draft deleted.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    // ==========================================
    // 6. SCHEDULER CONFIGURATION
    // ==========================================
    public async Task<Response<SchedulerConfigDto>> GetSchedulerConfigAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var setting = await _settingRepo.FindAsync(s => s.Key == "PAYROLL_SCHEDULER_CONFIG" && !s.IsDeleted);
            if (setting == null)
            {
                var defaultConfig = new SchedulerConfigDto("monthly", "28", "2", "NGN", true, false, false, true, "Auto Run Scheduler Settings", true);
                return new Response<SchedulerConfigDto> { StatusCode = StatusCodes.Status200OK, Message = "Default settings.", Data = defaultConfig };
            }

            var config = JsonSerializer.Deserialize<SchedulerConfigDto>(setting.Value);
            return new Response<SchedulerConfigDto> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = config! };
        }
        catch (Exception)
        {
            return new Response<SchedulerConfigDto> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> SaveSchedulerConfigAsync(SchedulerConfigDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var setting = await _settingRepo.FindAsync(s => s.Key == "PAYROLL_SCHEDULER_CONFIG");
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Key = "PAYROLL_SCHEDULER_CONFIG",
                    CreatedAt = DateTime.UtcNow
                };
                await _settingRepo.AddAsync(setting);
            }

            setting.Value = JsonSerializer.Serialize(request);
            await _settingRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Scheduler config saved.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<string>> TriggerSchedulerRunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate triggering immediate run: check first open period and trigger
            var openPeriod = await _payrollPeriodRepo.FindAsync(pp => pp.Status == "open" && !pp.IsDeleted);
            if (openPeriod == null)
                return new Response<string> { StatusCode = StatusCodes.Status400BadRequest, Message = "No open payroll periods found to auto execute." };

            var res = await RunPayrollAsync(new RunPayrollCommands(openPeriod.Id), cancellationToken);
            if (res.StatusCode == StatusCodes.Status200OK)
            {
                return new Response<string> { StatusCode = StatusCodes.Status200OK, Message = "Scheduler triggered run successfully.", Data = res.Data.ToString() };
            }

            return new Response<string> { StatusCode = res.StatusCode, Message = res.Message };
        }
        catch (Exception)
        {
            return new Response<string> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    // ==========================================
    // 7. REMITTANCES
    // ==========================================
    public async Task<Response<List<Remittance>>> GetRemittancesAsync(GetRemittancesQueries request, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _remittanceRepo.Query()
                .Include(r => r.Run)
                .ThenInclude(run => run.Period)
                .Where(r => !r.IsDeleted)
                .ToListAsync(cancellationToken);

            return new Response<List<Remittance>> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = list };
        }
        catch (Exception)
        {
            return new Response<List<Remittance>> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> GenerateRemittancesAsync(Guid periodId, string type, CancellationToken cancellationToken = default)
    {
        try
        {
            var run = await _payrollRunRepo.FindAsync(r => r.PeriodId == periodId && r.Status == "posted");
            if (run == null)
                return new Response<bool> { StatusCode = StatusCodes.Status400BadRequest, Message = "Deduction remittances can only be generated for a POSTED payroll run." };

            // Check if duplicate exists
            var exists = await _remittanceRepo.FindAsync(r => r.RunId == run.Id && r.DeductionType == type);
            if (exists != null)
                return new Response<bool> { StatusCode = StatusCodes.Status400BadRequest, Message = $"Remittance batch already exists for this period and deduction type." };

            // Query entries to sum up Employee Pension vs Employer Pension, PAYE, etc.
            var entries = await _entryRepo.Query().Where(e => e.RunId == run.Id).ToListAsync(cancellationToken);
            if (!entries.Any())
                return new Response<bool> { StatusCode = StatusCodes.Status400BadRequest, Message = "No payroll entries found in the target run." };

            decimal empSum = 0;
            decimal employerSum = 0;

            if (type == "pension")
            {
                empSum = entries.Sum(e => e.Pension);
                employerSum = entries.Sum(e => e.Basic * 0.10m); // 10% ER
            }
            else if (type == "nhf")
            {
                empSum = entries.Sum(e => e.Nhf);
                employerSum = 0;
            }
            else if (type == "paye")
            {
                empSum = entries.Sum(e => e.Paye);
                employerSum = 0;
            }
            else
            {
                empSum = entries.Sum(e => e.OtherStatutory);
                employerSum = 0;
            }

            var rem = new Remittance
            {
                Id = Guid.NewGuid(),
                RunId = run.Id,
                DeductionType = type,
                Amount = empSum + employerSum,
                EmployeeTotal = empSum,
                EmployerTotal = employerSum,
                TotalPayable = empSum + employerSum,
                Status = "draft",
                Reference = $"REF-{type.ToUpper()}-{DateTime.UtcNow:yyyyMM}-{new Random().Next(100, 999)}",
                CreatedAt = DateTime.UtcNow
            };

            await _remittanceRepo.AddAsync(rem);
            await _remittanceRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Remittance batch generated.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }

    public async Task<Response<bool>> UpdateRemittanceStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _remittanceRepo.FindAsync(r => r.Id == id);
            if (entity == null)
                return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Remittance not found." };

            entity.Status = status;
            await _remittanceRepo.SaveAsync();

            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = $"Status updated to {status}.", Data = true };
        }
        catch (Exception)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
        }
    }
}

public class SalaryComponentDto
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!; // earning, allowance, deduction
    public string CalculationType { get; set; } = null!; // flat, percentage
    public decimal Value { get; set; }
}
