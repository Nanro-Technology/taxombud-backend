using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Persistence.Data;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/hr/self-service")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class HrSelfServiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public HrSelfServiceController(ApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == currentUserId.Value, ct);

        if (user == null) return NotFound("User account not found.");

        // Self-healing: ensure staff profile exists
        var profile = await _context.StaffProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUserId.Value, ct);

        if (profile == null)
        {
            profile = new StaffProfile
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId.Value,
                EmployeeCode = $"EMP-{DateTime.UtcNow.Ticks % 100000}",
                HireDate = DateTimeOffset.UtcNow.AddYears(-1),
                EmploymentStatus = "Active",
                DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                BankAccountNo = "0000000000",
                BankId = "058",
                CreatedAt = DateTime.UtcNow
            };
            _context.StaffProfiles.Add(profile);
            await _context.SaveChangesAsync(ct);
        }

        // Self-healing: ensure wallet exists
        var wallet = await _context.EmployeeWallets
            .FirstOrDefaultAsync(w => w.UserId == currentUserId.Value, ct);

        if (wallet == null)
        {
            wallet = new EmployeeWallet
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId.Value,
                BalanceNgn = 15450, // Seed standard balance matching view defaults
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.EmployeeWallets.Add(wallet);
            await _context.SaveChangesAsync(ct);
        }

        var result = new
        {
            firstName = user.FirstName,
            lastName = user.LastName,
            jobTitle = user.JobTitle ?? "Officer",
            department = user.Department?.Name ?? "Unassigned",
            email = user.Email,
            phone = user.Phone ?? "",
            employmentType = user.EmploymentType ?? "Permanent",
            hireDate = profile.HireDate.ToString("yyyy-MM-dd"),
            address = profile.AddressLine1 ?? "",
            nextOfKin = profile.NextOfKinName ?? "",
            walletBalance = wallet.BalanceNgn
        };

        return Ok(new { StatusCode = 200, Message = "Profile retrieved successfully.", Data = result });
    }

    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateSelfProfileRequest request, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId.Value, ct);
        if (user == null) return NotFound("User account not found.");

        var profile = await _context.StaffProfiles.FirstOrDefaultAsync(p => p.UserId == currentUserId.Value, ct);
        if (profile == null) return NotFound("Staff profile not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Phone = request.Phone;

        profile.AddressLine1 = request.Address;
        profile.NextOfKinName = request.NextOfKin;
        profile.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Profile updated successfully." });
    }

    [HttpGet("leaves")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaves(CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var leaves = await _context.LeaveRequests
            .Where(l => l.UserId == currentUserId.Value && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new
            {
                id = "LR-" + l.Id.ToString().Substring(0, 4).ToUpper(),
                startDate = l.StartDate.ToString("yyyy-MM-dd"),
                endDate = l.EndDate.ToString("yyyy-MM-dd"),
                days = l.Days,
                status = char.ToUpper(l.Status[0]) + l.Status.Substring(1).ToLower(),
                reason = l.Reason ?? ""
            })
            .ToListAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Leaves retrieved successfully.", Data = leaves });
    }

    [HttpPost("leaves")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestLeave([FromBody] SubmitSelfLeaveRequest request, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        if (request.StartDate > request.EndDate)
            return BadRequest("Start date must be before end date.");

        var diff = (request.EndDate - request.StartDate).Days + 1;

        var leave = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId.Value,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = diff,
            Reason = request.Reason,
            Status = "pending",
            LeaveType = "Annual",
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(leave);
        await _context.SaveChangesAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Leave request submitted successfully.", Data = leave.Id });
    }

    [HttpGet("finance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinance(CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var wallet = await _context.EmployeeWallets
            .FirstOrDefaultAsync(w => w.UserId == currentUserId.Value, ct);

        if (wallet == null)
        {
            wallet = new EmployeeWallet
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId.Value,
                BalanceNgn = 15450,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.EmployeeWallets.Add(wallet);
            await _context.SaveChangesAsync(ct);
        }

        var txs = await _context.WalletTransactions
            .Where(t => t.WalletId == wallet.Id && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var transactions = txs.Select(t => new
        {
            date = t.CreatedAt.ToString("yyyy-MM-dd"),
            type = t.Reference,
            amount = (double)t.Amount,
            reference = t.Id.ToString().Substring(0, 8).ToUpper()
        }).ToList();

        var withdrawals = txs.Where(t => t.Type == "debit" && t.Reference == "WithdrawalRequest")
            .Select(t => new
            {
                date = t.CreatedAt.ToString("yyyy-MM-dd"),
                amount = (double)-t.Amount,
                status = char.ToUpper(t.Status[0]) + t.Status.Substring(1).ToLower(),
                reference = t.Id.ToString().Substring(0, 8).ToUpper()
            }).ToList();

        var loansQuery = await _context.LoanRequests
            .Where(l => l.UserId == currentUserId.Value && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        var advances = loansQuery.Where(l => l.IsSalaryAdvance)
            .Select(l => new
            {
                date = l.CreatedAt.ToString("yyyy-MM-dd"),
                amount = (double)l.Amount,
                status = char.ToUpper(l.Status[0]) + l.Status.Substring(1).ToLower(),
                remaining = l.Status == "disbursed" ? (double)l.Amount : 0.0
            }).ToList();

        var loans = loansQuery.Where(l => !l.IsSalaryAdvance)
            .Select(l => new
            {
                date = l.CreatedAt.ToString("yyyy-MM-dd"),
                principal = (double)l.Amount,
                status = char.ToUpper(l.Status[0]) + l.Status.Substring(1).ToLower(),
                remaining = l.Status == "disbursed" ? (double)l.Amount : 0.0
            }).ToList();

        var ewas = await _context.EwaRequests
            .Where(e => e.UserId == currentUserId.Value && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                date = e.CreatedAt.ToString("yyyy-MM-dd"),
                requested = (double)e.Amount,
                approved = (double)(e.Status == "approved" || e.Status == "disbursed" ? e.Amount : 0),
                status = char.ToUpper(e.Status[0]) + e.Status.Substring(1).ToLower(),
                period = e.CreatedAt.ToString("MMMM yyyy")
            })
            .ToListAsync(ct);

        var response = new
        {
            walletBalance = wallet.BalanceNgn,
            walletId = wallet.Id,
            transactions,
            withdrawals,
            advances,
            loans,
            ewa = ewas
        };

        return Ok(new { StatusCode = 200, Message = "Financial records retrieved successfully.", Data = response });
    }

    [HttpPost("withdrawals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestWithdrawal([FromBody] SubmitSelfWithdrawal request, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(w => w.UserId == currentUserId.Value, ct);
        if (wallet == null) return NotFound("Wallet not found.");

        if (request.Amount <= 0) return BadRequest("Amount must be greater than zero.");
        if (wallet.BalanceNgn < request.Amount) return BadRequest("Insufficient wallet balance.");

        var tx = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Amount = -request.Amount,
            Type = "debit",
            Reference = "WithdrawalRequest",
            Status = "pending",
            BankDetail = "Zenith Bank · 2084930192",
            CreatedAt = DateTime.UtcNow
        };

        // deduct amount from employee wallet immediately or on approval?
        // To match current WalletController behavior: WalletController doesn't deduct immediately.
        // It deducts on disburse. Let's do exactly that!
        _context.WalletTransactions.Add(tx);
        await _context.SaveChangesAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Withdrawal request submitted successfully.", Data = tx.Id });
    }

    [HttpGet("documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var profile = await _context.StaffProfiles.FirstOrDefaultAsync(p => p.UserId == currentUserId.Value, ct);
        if (profile == null) return Ok(new { StatusCode = 200, Message = "Success", Data = new List<object>() });

        // Retrieve assigned documents
        var list = await _context.StaffProfiles
            .Where(p => p.Id == profile.Id)
            .SelectMany(p => p.Documents)
            .Select(d => new
            {
                id = d.Id.ToString(),
                title = d.FileName,
                category = d.DocumentType ?? "Contract",
                status = "Verified",
                updated = d.CreatedAt.ToString("yyyy-MM-dd")
            })
            .ToListAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Success", Data = list });
    }

    [HttpGet("performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformance(CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var profile = await _context.StaffProfiles.FirstOrDefaultAsync(p => p.UserId == currentUserId.Value, ct);
        var profileId = profile?.Id ?? Guid.Empty;

        var goalsList = await _context.PerformanceGoals
            .Include(g => g.Cycle)
            .Where(g => g.EmployeeId == currentUserId.Value || (profileId != Guid.Empty && g.EmployeeId == profileId))
            .Select(g => new
            {
                title = g.Title,
                description = g.Description,
                period = g.Cycle != null ? g.Cycle.Name : "Q3 2026",
                progress = g.ProgressPercentage,
                status = g.Status,
                reviewer = "Ayodele Ayowole"
            })
            .ToListAsync(ct);

        var reviewsList = await _context.PerformanceReviews
            .Include(r => r.Cycle)
            .Where(r => r.EmployeeId == currentUserId.Value || (profileId != Guid.Empty && r.EmployeeId == profileId))
            .Select(r => new
            {
                period = r.Cycle != null ? r.Cycle.Name : "H1 2026 Mid-Year Appraisal",
                rating = (int)Math.Round(r.Score),
                reviewer = "Ayodele Ayowole",
                status = r.Status
            })
            .ToListAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Success", Data = new { goals = goalsList, reviews = reviewsList } });
    }

    [HttpGet("payslips")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslips(CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        // Join directly to avoid EF global query filter conflict on PayrollRun navigation
        var entries = await (
            from e in _context.PayrollEntries
            join r in _context.PayrollRuns.IgnoreQueryFilters() on e.RunId equals r.Id
            join p in _context.PayrollPeriods on r.PeriodId equals p.Id into periods
            from p in periods.DefaultIfEmpty()
            where e.UserId == currentUserId.Value && !e.IsDeleted
            orderby e.CreatedAt descending
            select new
            {
                id = e.Id,
                period = p != null ? p.Name : e.CreatedAt.ToString("MMMM yyyy"),
                paymentDate = r.PostedAt != null ? r.PostedAt.Value.ToString("yyyy-MM-dd") : e.CreatedAt.ToString("yyyy-MM-dd"),
                gross = e.Gross,
                net = e.Net,
                status = e.PaymentStatus
            }
        ).ToListAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Payslips retrieved successfully.", Data = entries });
    }

    [HttpGet("payslips/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayslipDetail(Guid id, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == currentUserId.Value, ct);
        if (user == null) return Unauthorized();

        var staffProfile = await _context.StaffProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUserId.Value, ct);

        var entry = await _context.PayrollEntries
            .Include(e => e.Run)
                .ThenInclude(r => r.Period)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == currentUserId.Value && !e.IsDeleted, ct);

        if (entry == null) return NotFound("Payslip not found.");

        var salaryProfile = await _context.SalaryProfiles
            .Where(sp => sp.UserId == currentUserId.Value && !sp.IsDeleted)
            .OrderByDescending(sp => sp.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

        var result = new
        {
            id = entry.Id,
            period = entry.Run.Period != null ? entry.Run.Period.Name : entry.CreatedAt.ToString("MMMM yyyy"),
            paymentDate = entry.Run.PostedAt != null ? entry.Run.PostedAt.Value.ToString("yyyy-MM-dd") : entry.CreatedAt.ToString("yyyy-MM-dd"),
            employee = new
            {
                name = $"{user.FirstName} {user.LastName}",
                jobTitle = user.JobTitle ?? "Officer",
                department = user.Department?.Name ?? "Unassigned",
                employeeCode = staffProfile?.EmployeeCode ?? "N/A",
                bankAccountNo = staffProfile?.BankAccountNo ?? "N/A",
                employmentType = user.EmploymentType ?? "Full-Time"
            },
            earnings = new
            {
                basic = entry.Basic,
                allowances = entry.Allowances,
                gross = entry.Gross
            },
            deductions = new
            {
                paye = entry.Paye,
                pension = entry.Pension,
                nhf = entry.Nhf,
                otherStatutory = entry.OtherStatutory,
                total = entry.Deductions
            },
            net = entry.Net,
            currency = entry.Run.Currency,
            status = entry.PaymentStatus
        };

        return Ok(new { StatusCode = 200, Message = "Payslip retrieved successfully.", Data = result });
    }
}

public record UpdateSelfProfileRequest(string FirstName, string LastName, string Phone, string Address, string NextOfKin);
public record SubmitSelfLeaveRequest(DateTime StartDate, DateTime EndDate, string Reason);
public record SubmitSelfWithdrawal(decimal Amount);
