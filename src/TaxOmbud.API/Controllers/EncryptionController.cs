using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.InfrastructureService;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Endpoints for End-to-End Encryption key exchange.
/// </summary>
[AllowAnonymous]
[Route("api/v1/encryption")]
public class EncryptionController : ControllerBase
{
    private readonly IEncryptionService _encryptionService;

    public EncryptionController(
        IEncryptionService encryptionService
    )
    {
        _encryptionService = encryptionService;
    }



    /// <summary>Get the server's RSA Public Key for E2EE.</summary>
    [HttpGet("public-key")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPublicKey()
    {
        var pem = _encryptionService.GetPublicKeyPem();
        return Ok(new { PublicKey = pem });
    }
}
