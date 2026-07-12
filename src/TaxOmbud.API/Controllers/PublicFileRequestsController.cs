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
[Route("api/v1/public-file-requests")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class PublicFileRequestsController : ControllerBase
{
    private readonly IPublicFileRequestService _publicFileRequestService;

    public PublicFileRequestsController(IPublicFileRequestService publicFileRequestService)
    {
        _publicFileRequestService = publicFileRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicFileRequests(CancellationToken ct)
    {
        var result = await _publicFileRequestService.GetPublicFileRequestsAsync(ct);
        return Ok(new Response<List<PublicFileRequest>> { StatusCode = 200, Message = "Success", Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePublicFileRequest([FromBody] CreatePublicFileRequestApiRequest request, CancellationToken ct)
    {
        var result = await _publicFileRequestService.CreatePublicFileRequestAsync(
            request.Name, 
            request.ExpiresAt, 
            request.AllowedExtensions, 
            request.MaxSizeMb, 
            request.NotifyEmails, 
            request.Notes, 
            ct);
        return Ok(new Response<PublicFileRequest> { StatusCode = 200, Message = "Success", Data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePublicFileRequest(Guid id, CancellationToken ct)
    {
        var success = await _publicFileRequestService.DeletePublicFileRequestAsync(id, ct);
        if (!success)
            return NotFound(new Response<object> { StatusCode = 404, Message = "File request not found" });
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPublicFileRequestById(Guid id, CancellationToken ct)
    {
        var result = await _publicFileRequestService.GetPublicFileRequestByIdAsync(id, ct);
        if (result == null)
            return NotFound(new Response<object> { StatusCode = 404, Message = "File request not found" });
        return Ok(new Response<PublicFileRequest> { StatusCode = 200, Message = "Success", Data = result });
    }

    [HttpPost("{id:guid}/upload")]
    [AllowAnonymous] // Allow external users to upload files without logging in
    public async Task<IActionResult> UploadFileToRequest(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new Response<object> { StatusCode = 400, Message = "No file uploaded" });

        using var stream = file.OpenReadStream();
        var result = await _publicFileRequestService.UploadFileToRequestAsync(id, file.FileName, stream, file.ContentType, ct);
        return Ok(new Response<PublicFileRequestUpload> { StatusCode = 200, Message = "Success", Data = result });
    }
}

public class CreatePublicFileRequestApiRequest
{
    public string Name { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public List<string> AllowedExtensions { get; set; } = new();
    public int MaxSizeMb { get; set; } = 10;
    public string NotifyEmails { get; set; } = null!;
    public string? Notes { get; set; }
}
