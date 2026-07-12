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
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Persistence.Data;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts(CancellationToken ct)
    {
        var rawAccounts = await _context.Accounts
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

        var accounts = rawAccounts.Select(a => new
        {
            id = a.Id.ToString(),
            name = a.Name,
            avatarText = GetAvatarText(a.Name),
            avatarBg = GetAvatarBg(a.Name),
            health = a.HealthScore,
            phone = a.Phone ?? "",
            altPhone = a.AltPhone ?? "",
            email = a.Email ?? "",
            website = a.Website ?? "",
            country = a.Country,
            status = a.Status,
            description = a.Description ?? "",
            address = a.Address ?? "",
            state = a.State ?? "",
            city = a.City ?? "",
            postalCode = a.PostalCode ?? "",
            industry = a.Industry ?? "Tax Ombud"
        }).ToList();

        return Ok(new { StatusCode = 200, Message = "Accounts retrieved successfully.", Data = accounts });
    }

    [HttpGet("{id:guid}", Name = "GetAccountById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(Guid id, CancellationToken ct)
    {
        var a = await _context.Accounts.FirstOrDefaultAsync(acc => acc.Id == id && !acc.IsDeleted, ct);
        if (a == null) return NotFound("Account not found.");

        var result = new
        {
            id = a.Id.ToString(),
            name = a.Name,
            avatarText = GetAvatarText(a.Name),
            avatarBg = GetAvatarBg(a.Name),
            health = a.HealthScore,
            phone = a.Phone ?? "",
            altPhone = a.AltPhone ?? "",
            email = a.Email ?? "",
            website = a.Website ?? "",
            country = a.Country,
            status = a.Status,
            description = a.Description ?? "",
            address = a.Address ?? "",
            state = a.State ?? "",
            city = a.City ?? "",
            postalCode = a.PostalCode ?? "",
            industry = a.Industry ?? "Tax Ombud"
        };

        return Ok(new { StatusCode = 200, Message = "Account details retrieved successfully.", Data = result });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateOrUpdateAccountRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Account name is required.");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Country = request.Country ?? "Nigeria",
            Status = request.Status ?? "Active",
            HealthScore = 70, // Default health for new accounts
            Description = request.Description,
            Website = request.Website,
            AltPhone = request.AltPhone,
            Address = request.Address,
            State = request.State,
            City = request.City,
            PostalCode = request.PostalCode,
            Industry = request.Industry ?? "Tax Ombud",
            IsWorkflowLane = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(ct);

        var responseData = new { id = account.Id };
        return CreatedAtRoute("GetAccountById", new { id = account.Id }, new { StatusCode = 201, Message = "Account created successfully.", Data = responseData });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] CreateOrUpdateAccountRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Account name is required.");

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        if (account == null) return NotFound("Account not found.");

        account.Name = request.Name;
        account.Phone = request.Phone;
        account.Email = request.Email;
        account.Country = request.Country ?? "Nigeria";
        account.Status = request.Status ?? "Active";
        account.Description = request.Description;
        account.Website = request.Website;
        account.AltPhone = request.AltPhone;
        account.Address = request.Address;
        account.State = request.State;
        account.City = request.City;
        account.PostalCode = request.PostalCode;
        account.Industry = request.Industry ?? "Tax Ombud";
        account.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Account updated successfully." });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken ct)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        if (account == null) return NotFound("Account not found.");

        account.IsDeleted = true;
        account.DeletedAt = DateTimeOffset.UtcNow;
        account.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Account deleted successfully." });
    }

    private static string GetAvatarBg(string name)
    {
        var colors = new[] { "#dc2626", "#4f46e5", "#db2777", "#16a34a", "#8b5cf6", "#ea580c" };
        var index = Math.Abs(name.GetHashCode()) % colors.Length;
        return colors[index];
    }

    private static string GetAvatarText(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Select(w => w[0].ToString()).Aggregate("", (current, next) => current + next);
        return initials.Substring(0, Math.Min(2, initials.Length)).ToUpper();
    }
}

public record CreateOrUpdateAccountRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Country,
    string? Status,
    string? Description,
    string? Website,
    string? AltPhone,
    string? Address,
    string? State,
    string? City,
    string? PostalCode,
    string? Industry
);
