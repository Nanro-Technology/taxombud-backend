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
using TaxOmbud.Application.SecuredFiling.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/secured-filing")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class SecuredFilingController : ControllerBase
{
    private readonly ISecuredFilingService _filingService;

    public SecuredFilingController(ISecuredFilingService filingService)
    {
        _filingService = filingService;
    }

    /// <summary>List folders in the registry with optional search.</summary>
    [HttpGet("folders")]
    public async Task<IActionResult> GetFolders([FromQuery] string? query, CancellationToken ct)
    {
        var result = await _filingService.GetFoldersAsync(query, ct);
        return Ok(new Response<List<FilingFolderDto>> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Get details of a folder by ID.</summary>
    [HttpGet("folders/{id:guid}")]
    public async Task<IActionResult> GetFolderById(Guid id, CancellationToken ct)
    {
        var result = await _filingService.GetFolderByIdAsync(id, ct);
        if (result == null)
            return NotFound(new Response<FilingFolderDto> { StatusCode = 404, Message = "Folder not found" });
        return Ok(new Response<FilingFolderDto> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Create and register a new folder.</summary>
    [HttpPost("folders")]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request, CancellationToken ct)
    {
        var result = await _filingService.CreateFolderAsync(request, ct);
        return Ok(new Response<FilingFolderDto> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Bulk delete selected folders.</summary>
    [HttpDelete("folders")]
    public async Task<IActionResult> DeleteFolders([FromBody] List<Guid> folderIds, CancellationToken ct)
    {
        var success = await _filingService.DeleteFoldersAsync(folderIds, ct);
        if (!success)
            return BadRequest(new Response<object> { StatusCode = 400, Message = "No folders deleted" });
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }

    /// <summary>List documents/files inside folders with optional search.</summary>
    [HttpGet("files")]
    public async Task<IActionResult> GetFiles([FromQuery] Guid? folderId, [FromQuery] string? query, CancellationToken ct)
    {
        var result = await _filingService.GetDocumentsAsync(folderId, query, ct);
        return Ok(new Response<List<FilingDocumentDto>> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Upload a file to a folder.</summary>
    [HttpPost("files")]
    public async Task<IActionResult> UploadFile(
        [FromForm] Guid folderId, 
        [FromForm] string? sender, 
        [FromForm] string? senderOrg, 
        IFormFile file, 
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new Response<FilingDocumentDto> { StatusCode = 400, Message = "No file uploaded" });

        using var stream = file.OpenReadStream();
        var result = await _filingService.UploadDocumentAsync(folderId, file.FileName, stream, file.ContentType, sender, senderOrg, ct);
        return Ok(new Response<FilingDocumentDto> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Bulk delete selected files.</summary>
    [HttpDelete("files")]
    public async Task<IActionResult> DeleteFiles([FromBody] List<Guid> documentIds, CancellationToken ct)
    {
        var success = await _filingService.DeleteDocumentsAsync(documentIds, ct);
        if (!success)
            return BadRequest(new Response<object> { StatusCode = 400, Message = "No files deleted" });
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }

    /// <summary>List routed items in the filing inbox queue.</summary>
    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox([FromQuery] string? query, CancellationToken ct)
    {
        var result = await _filingService.GetInboxRoutingsAsync(query, ct);
        return Ok(new Response<List<FilingInboxRoutingDto>> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Acknowledge a routing request.</summary>
    [HttpPost("inbox/{id:guid}/acknowledge")]
    public async Task<IActionResult> AcknowledgeRouting(Guid id, CancellationToken ct)
    {
        var success = await _filingService.AcknowledgeRoutingAsync(id, ct);
        if (!success)
            return NotFound(new Response<object> { StatusCode = 404, Message = "Routing item not found" });
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }

    /// <summary>Reject routing of a folder.</summary>
    [HttpPost("inbox/{id:guid}/reject")]
    public async Task<IActionResult> RejectRouting(Guid id, [FromBody] RejectRoutingRequest request, CancellationToken ct)
    {
        var success = await _filingService.RejectRoutingAsync(id, request.Reason, ct);
        if (!success)
            return NotFound(new Response<object> { StatusCode = 404, Message = "Routing item not found" });
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }

    /// <summary>List document categories.</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] string? query, CancellationToken ct)
    {
        var result = await _filingService.GetCategoriesAsync(query, ct);
        return Ok(new Response<List<FilingCategoryDto>> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Create a new category.</summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _filingService.CreateCategoryAsync(request, ct);
        return Ok(new Response<FilingCategoryDto> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Delete a category.</summary>
    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        var success = await _filingService.DeleteCategoryAsync(id, ct);
        if (!success)
            return NotFound(new Response<object> { StatusCode = 404, Message = "Category not found" });
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }

    /// <summary>List audit activities in the secured filing system.</summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(CancellationToken ct)
    {
        var result = await _filingService.GetSecuredFilingAuditLogsAsync(ct);
        return Ok(new Response<List<AuditLog>> { StatusCode = 200, Message = "Success", Data = result });
    }

    /// <summary>Clear all secured filing logs.</summary>
    [HttpDelete("audit-logs")]
    public async Task<IActionResult> ClearAuditLogs(CancellationToken ct)
    {
        await _filingService.ClearSecuredFilingAuditLogsAsync(ct);
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }
}
