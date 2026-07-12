using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Wallet.DTOs;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/wallet")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly TaxOmbud.Application.Interfaces.Persistence.IApplicationDbContext _context;

    public WalletController(IWalletService walletService, TaxOmbud.Application.Interfaces.Persistence.IApplicationDbContext context)
    {
        _walletService = walletService;
        _context = context;
    }

    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWalletBalance([FromQuery] GetWalletBalanceQueries query, CancellationToken ct)
    {
        var result = await _walletService.GetWalletBalanceAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWalletTransactions([FromQuery] GetWalletTransactionsQueries query, CancellationToken ct)
    {
        var result = await _walletService.GetWalletTransactionsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("withdrawals/request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestWithdrawal([FromBody] RequestWithdrawalCommands command, CancellationToken ct)
    {
        // Intercept and store BankDetail
        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(w => w.Id == command.WalletId, ct);
        if (wallet == null) return NotFound("Wallet not found.");

        var tx = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = command.WalletId,
            Amount = -command.Amount,
            Type = "debit",
            Reference = "WithdrawalRequest",
            Status = "pending",
            BankDetail = "Zenith Bank · 2084930192", // Seed a default bank details to display
            CreatedAt = DateTime.UtcNow
        };

        _context.WalletTransactions.Add(tx);
        await _context.SaveChangesAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Withdrawal request submitted successfully.", Data = tx.Id });
    }

    // ─── ADMIN WALLET MANAGEMENT ──────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWallets(CancellationToken ct)
    {
        // Self-healing: ensure all staff users have wallets
        var users = await _context.Users.ToListAsync(ct);
        foreach (var u in users)
        {
            var walletExists = await _context.EmployeeWallets.AnyAsync(w => w.UserId == u.Id, ct);
            if (!walletExists)
            {
                _context.EmployeeWallets.Add(new EmployeeWallet 
                { 
                    Id = Guid.NewGuid(), 
                    UserId = u.Id, 
                    BalanceNgn = 50000, // Seed with 50,000 NGN balance to make it testable
                    Status = "active" 
                });
            }
        }
        await _context.SaveChangesAsync(ct);

        var list = await _context.EmployeeWallets
            .Include(w => w.User)
            .Select(w => new {
                Id = w.Id,
                UserId = w.UserId,
                EmployeeName = w.User.FullName,
                Balance = w.BalanceNgn,
                Currency = "NGN",
                Status = w.Status,
                PendingAmount = w.Transactions.Where(t => t.Type == "debit" && t.Status == "pending").Sum(t => t.Amount) * -1,
                LastUpdated = w.LastModifiedAt.HasValue ? w.LastModifiedAt.Value.ToString("yyyy-MM-dd HH:mm") : w.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
            .ToListAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Wallets retrieved successfully.", Data = list });
    }

    [HttpPost("{id:guid}/credit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreditWallet(Guid id, [FromBody] CreditWalletRequest request, CancellationToken ct)
    {
        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (wallet == null) return NotFound("Wallet not found.");

        wallet.BalanceNgn += request.Amount;
        _context.WalletTransactions.Add(new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Type = "credit",
            Amount = request.Amount,
            Reference = string.IsNullOrWhiteSpace(request.Reference) ? "Admin Credit" : request.Reference,
            Status = "paid",
            PaidAt = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Wallet credited successfully." });
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateWalletStatusRequest request, CancellationToken ct)
    {
        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (wallet == null) return NotFound("Wallet not found.");

        wallet.Status = request.Status;
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Wallet status updated successfully." });
    }

    [HttpGet("withdrawals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWithdrawals(CancellationToken ct)
    {
        var list = await _context.WalletTransactions
            .Include(t => t.Wallet)
                .ThenInclude(w => w.User)
            .Where(t => t.Type == "debit" && t.Reference == "WithdrawalRequest")
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new {
                Id = t.Id,
                EmployeeName = t.Wallet.User.FullName,
                Amount = t.Amount * -1,
                BankDetail = t.BankDetail ?? "Zenith Bank · 2084930192",
                Status = t.Status,
                RequestedAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                ApprovedAt = t.ApprovedAt.HasValue ? t.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm") : null,
                PaidAt = t.PaidAt.HasValue ? t.PaidAt.Value.ToString("yyyy-MM-dd HH:mm") : null
            })
            .ToListAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Withdrawals retrieved successfully.", Data = list });
    }

    [HttpGet("payout-attempts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayoutAttempts(CancellationToken ct)
    {
        var list = await _context.WalletTransactions
            .Include(t => t.Wallet)
                .ThenInclude(w => w.User)
            .Where(t => t.Type == "debit" && t.Reference == "WithdrawalRequest")
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new {
                Id = "PA-" + t.Id.ToString().Substring(0, 8),
                WithdrawalRef = t.Id.ToString().Substring(0, 8),
                EmployeeName = t.Wallet.User.FullName,
                Amount = t.Amount * -1,
                Status = t.Status == "paid" ? "success" : t.Status == "rejected" ? "failed" : "processing",
                AttemptNumber = t.AttemptNumber,
                ProviderRef = t.ProviderRef ?? "TXN-PENDING",
                WalletDebited = t.Status == "paid" || t.Status == "approved",
                InitiatedAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                CompletedAt = t.PaidAt.HasValue ? t.PaidAt.Value.ToString("yyyy-MM-dd HH:mm") : null
            })
            .ToListAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Payout attempts retrieved successfully.", Data = list });
    }

    [HttpPost("withdrawals/{id:guid}/approve")]
    public async Task<IActionResult> ApproveWithdrawal(Guid id, CancellationToken ct)
    {
        var tx = await _context.WalletTransactions.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tx == null) return NotFound("Withdrawal not found.");

        tx.Status = "approved";
        tx.ApprovedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Withdrawal approved successfully." });
    }

    [HttpPost("withdrawals/{id:guid}/reject")]
    public async Task<IActionResult> RejectWithdrawal(Guid id, CancellationToken ct)
    {
        var tx = await _context.WalletTransactions.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tx == null) return NotFound("Withdrawal not found.");

        tx.Status = "rejected";
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Withdrawal rejected successfully." });
    }

    [HttpPost("withdrawals/{id:guid}/disburse")]
    public async Task<IActionResult> DisburseWithdrawal(Guid id, CancellationToken ct)
    {
        var tx = await _context.WalletTransactions.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tx == null) return NotFound("Withdrawal not found.");

        tx.Status = "paid";
        tx.PaidAt = DateTimeOffset.UtcNow;
        tx.ProviderRef = "TXN-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        // deduct amount from employee wallet
        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(w => w.Id == tx.WalletId, ct);
        if (wallet != null)
        {
            wallet.BalanceNgn += tx.Amount; // tx.Amount is negative
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Withdrawal disbursed successfully." });
    }
}

public record CreditWalletRequest(decimal Amount, string? Reference);
public record UpdateWalletStatusRequest(string Status);
