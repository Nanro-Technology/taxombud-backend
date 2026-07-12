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
[Route("api/v1/file-manager")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class FileManagerController : ControllerBase
{
    private readonly IFileManagerService _fileManagerService;

    public FileManagerController(IFileManagerService fileManagerService)
    {
        _fileManagerService = fileManagerService;
    }

    [HttpGet("files")]
    public async Task<IActionResult> GetFiles([FromQuery] string? path, CancellationToken ct)
    {
        var result = await _fileManagerService.GetFilesAsync("", path ?? "My Files", ct);
        return Ok(new Response<List<UserFile>> { StatusCode = 200, Message = "Success", Data = result });
    }

    [HttpPost("folders")]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderApiRequest request, CancellationToken ct)
    {
        var result = await _fileManagerService.CreateFolderAsync("", request.Path, request.Name, ct);
        return Ok(new Response<UserFile> { StatusCode = 200, Message = "Success", Data = result });
    }

    [HttpPost("files")]
    public async Task<IActionResult> UploadFile([FromForm] string path, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new Response<object> { StatusCode = 400, Message = "No file uploaded" });

        using var stream = file.OpenReadStream();
        var result = await _fileManagerService.UploadFileAsync("", path, file.FileName, stream, file.ContentType, ct);
        return Ok(new Response<UserFile> { StatusCode = 200, Message = "Success", Data = result });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteItems([FromBody] List<Guid> ids, CancellationToken ct)
    {
        var success = await _fileManagerService.DeleteItemsAsync(ids, ct);
        if (!success)
            return BadRequest(new Response<object> { StatusCode = 400, Message = "No items deleted" });
        return Ok(new Response<object> { StatusCode = 200, Message = "Success" });
    }

    [HttpGet("files/{id:guid}/preview")]
    public async Task<IActionResult> GetFilePreview(Guid id, CancellationToken ct)
    {
        var file = await _fileManagerService.GetFileByIdAsync(id, ct);
        if (file == null)
            return NotFound(new Response<object> { StatusCode = 404, Message = "File not found" });

        return Ok(new Response<UserFile> { StatusCode = 200, Message = "Success", Data = file });
    }
}

public class CreateFolderApiRequest
{
    public string Path { get; set; } = null!;
    public string Name { get; set; } = null!;
}
