using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/sign-requests")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class SignRequestsController : ControllerBase
{
    private readonly ISignRequestService _signRequestService;

    public SignRequestsController(ISignRequestService signRequestService)
    {
        _signRequestService = signRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSignRequests(CancellationToken ct)
    {
        var result = await _signRequestService.GetSignRequestsAsync(ct);
        return Ok(new Response<List<SignRequest>> { StatusCode = 200, Message = "Success", Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSignRequest([FromForm] string signatoryEmail, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new Response<object> { StatusCode = 400, Message = "No PDF file uploaded" });

        using var stream = file.OpenReadStream();
        var result = await _signRequestService.CreateSignRequestAsync(file.FileName, stream, file.ContentType, signatoryEmail, ct);
        return Ok(new Response<SignRequest> { StatusCode = 200, Message = "Success", Data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSignRequest(Guid id, CancellationToken ct)
    {
        var success = await _signRequestService.DeleteSignRequestAsync(id, ct);
        if (!success)
            return NotFound(new Response<object> { StatusCode = 404, Message = "Request not found" });
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }

    [HttpPost("{id:guid}/sign")]
    [AllowAnonymous] // Allow external signature capture page without full app login
    public async Task<IActionResult> SignRequest(Guid id, IFormFile signature, CancellationToken ct)
    {
        if (signature == null || signature.Length == 0)
            return BadRequest(new Response<object> { StatusCode = 400, Message = "No signature image uploaded" });

        using var stream = signature.OpenReadStream();
        var result = await _signRequestService.SignRequestAsync(id, stream, ct);
        if (result == null)
            return NotFound(new Response<object> { StatusCode = 404, Message = "Sign request not found" });

        return Ok(new Response<SignRequest> { StatusCode = 200, Message = "Success", Data = result });
    }
}
