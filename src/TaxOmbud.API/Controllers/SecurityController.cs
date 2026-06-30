using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecurityController : ControllerBase
{
    private readonly ICryptoService _cryptoService;
    private readonly TaxOmbud.Application.Common.Interfaces.IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public SecurityController(
        ICryptoService cryptoService,
        TaxOmbud.Application.Common.Interfaces.IApplicationDbContext context,
        ICacheService cache
    )
    {
        _cryptoService = cryptoService;
        _context = context;
        _cache = cache;
    }

    /// <summary>Get the server's public RSA key for End-to-End Encryption.</summary>
    [HttpGet("public-key")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPublicKey()
    {
        return Ok(new
        {
            publicKey = _cryptoService.GetPublicKeyPem(),
            format = "PKCS8",
            algorithm = "RSA-2048"
        });
    }

    /// <summary>Check if E2EE is currently enforced.</summary>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var isEnabled = await GetE2eeStatusAsync(ct);
        return Ok(new { e2eeEnabled = isEnabled });
    }

    /// <summary>Admin endpoint to toggle E2EE enforcement.</summary>
    [HttpPost("toggle")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleE2ee([FromBody] ToggleRequest request, CancellationToken ct)
    {
        var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "Security:E2EE_Enabled", ct);
        if (setting == null)
        {
            setting = new SystemSetting
            {
                Key = "Security:E2EE_Enabled",
                Value = request.Enable ? "true" : "false",
                Description = "Toggles End-to-End Encryption (E2EE) for the API"
            };
            _context.SystemSettings.Add(setting);
        }
        else
        {
            setting.Value = request.Enable ? "true" : "false";
        }

        await _context.SaveChangesAsync(ct);
        await _cache.RemoveAsync("E2EE_Enabled", ct); // Invalidate cache

        return Ok(new { e2eeEnabled = request.Enable });
    }

    private async Task<bool> GetE2eeStatusAsync(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<string>("E2EE_Enabled", ct);
        if (cached != null) return cached == "true";

        var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "Security:E2EE_Enabled", ct);
        var isEnabled = setting?.Value == "true";

        // Cache for 5 minutes
        await _cache.SetAsync("E2EE_Enabled", isEnabled ? "true" : "false", System.TimeSpan.FromMinutes(5), ct);
        return isEnabled;
    }
}

public class ToggleRequest
{
    public bool Enable { get; set; }
}
