using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Common.Interfaces;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Endpoints for End-to-End Encryption key exchange.
/// </summary>
[AllowAnonymous]
[Route("api/v1/encryption")]
public class EncryptionController : ApiControllerBase
{
    private readonly IEncryptionService _encryptionService;

    public EncryptionController(IEncryptionService encryptionService)
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
