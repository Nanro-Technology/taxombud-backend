using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.System.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/forms")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FormsController : ControllerBase
{
    private const string FormsConfigKey = "digitalforms.config";
    private const string SubmissionsConfigKey = "formsubmissions.config";
    private readonly ISystemService _systemService;

    public FormsController(ISystemService systemService)
    {
        _systemService = systemService;
    }

    private async Task<List<JsonElement>> GetListAsync(string key, CancellationToken ct)
    {
        var settingsRes = await _systemService.GetSettingsAsync(new GetSettingsQuery(), ct);
        var settings = settingsRes.Data ?? new List<Domain.Entities.System.SystemSetting>();
        var entry = settings.FirstOrDefault(s => s.Key == key);
        if (entry != null && !string.IsNullOrWhiteSpace(entry.Value))
        {
            try
            {
                return JsonSerializer.Deserialize<List<JsonElement>>(entry.Value) ?? new List<JsonElement>();
            }
            catch
            {
                return new List<JsonElement>();
            }
        }
        return new List<JsonElement>();
    }

    private async Task SaveListAsync(string key, List<JsonElement> list, string description, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(list);
        await _systemService.UpdateSettingAsync(new UpdateSettingCommand(key, json, description), ct);
    }

    // ─── Digital Forms CRUD ───────────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(Response<List<JsonElement>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForms(CancellationToken ct)
    {
        var list = await GetListAsync(FormsConfigKey, ct);
        var response = new Response<List<JsonElement>>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Forms retrieved successfully.",
            Data = list
        };
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Response<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFormById(string id, CancellationToken ct)
    {
        var list = await GetListAsync(FormsConfigKey, ct);
        var form = list.FirstOrDefault(f => f.GetProperty("id").GetString() == id);
        if (form.ValueKind == JsonValueKind.Undefined)
        {
            return NotFound(new Response<object> { StatusCode = StatusCodes.Status404NotFound, Message = $"Form with ID {id} not found." });
        }
        var response = new Response<JsonElement>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Form retrieved successfully.",
            Data = form
        };
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Response<JsonElement>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateForm([FromBody] JsonElement payload, CancellationToken ct)
    {
        var list = await GetListAsync(FormsConfigKey, ct);

        // Map payload to dictionary so we can append properties
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(payload.GetRawText()) ?? new Dictionary<string, object>();
        
        string id = dict.ContainsKey("id") ? dict["id"]?.ToString() ?? "" : "";
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString("N");
            dict["id"] = id;
        }

        if (!dict.ContainsKey("status")) dict["status"] = "Draft";
        if (!dict.ContainsKey("lastActivity")) dict["lastActivity"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        var updatedElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict));
        list.Insert(0, updatedElement);

        await SaveListAsync(FormsConfigKey, list, "Digital forms configurations list", ct);
        
        var response = new Response<JsonElement>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Form created successfully.",
            Data = updatedElement
        };
        return Ok(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Response<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateForm(string id, [FromBody] JsonElement payload, CancellationToken ct)
    {
        var list = await GetListAsync(FormsConfigKey, ct);
        var index = list.FindIndex(f => f.GetProperty("id").GetString() == id);
        if (index == -1)
        {
            return NotFound(new Response<object> { StatusCode = StatusCodes.Status404NotFound, Message = $"Form with ID {id} not found." });
        }

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(payload.GetRawText()) ?? new Dictionary<string, object>();
        dict["id"] = id;
        dict["lastActivity"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        var updatedElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict));
        list[index] = updatedElement;

        await SaveListAsync(FormsConfigKey, list, "Digital forms configurations list", ct);

        var response = new Response<JsonElement>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Form updated successfully.",
            Data = updatedElement
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteForm(string id, CancellationToken ct)
    {
        var list = await GetListAsync(FormsConfigKey, ct);
        var index = list.FindIndex(f => f.GetProperty("id").GetString() == id);
        if (index == -1)
        {
            return NotFound(new Response<object> { StatusCode = StatusCodes.Status404NotFound, Message = $"Form with ID {id} not found." });
        }

        list.RemoveAt(index);
        await SaveListAsync(FormsConfigKey, list, "Digital forms configurations list", ct);

        // Cascade delete submissions
        var submissions = await GetListAsync(SubmissionsConfigKey, ct);
        var remainingSubmissions = submissions.Where(s => s.GetProperty("formId").GetString() != id).ToList();
        await SaveListAsync(SubmissionsConfigKey, remainingSubmissions, "Filled digital form submissions logs", ct);

        var response = new Response<object?>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = $"Form {id} and all its submissions deleted successfully."
        };
        return Ok(response);
    }

    // ─── Submissions CRUD ─────────────────────────────────────────────────────

    [HttpGet("{formId}/submissions")]
    [ProducesResponseType(typeof(Response<List<JsonElement>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubmissions(string formId, CancellationToken ct)
    {
        var list = await GetListAsync(SubmissionsConfigKey, ct);
        var filtered = list.Where(s => s.GetProperty("formId").GetString() == formId).ToList();

        var response = new Response<List<JsonElement>>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Submissions retrieved successfully.",
            Data = filtered
        };
        return Ok(response);
    }

    [HttpPost("{formId}/submissions")]
    [ProducesResponseType(typeof(Response<JsonElement>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitRecord(string formId, [FromBody] JsonElement payload, CancellationToken ct)
    {
        var list = await GetListAsync(SubmissionsConfigKey, ct);

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(payload.GetRawText()) ?? new Dictionary<string, object>();
        dict["id"] = Guid.NewGuid().ToString("N");
        dict["formId"] = formId;
        dict["submittedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        var newElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict));
        list.Insert(0, newElement);

        await SaveListAsync(SubmissionsConfigKey, list, "Filled digital form submissions logs", ct);

        var response = new Response<JsonElement>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Submission recorded successfully.",
            Data = newElement
        };
        return Ok(response);
    }

    [HttpDelete("{formId}/submissions/{id}")]
    [ProducesResponseType(typeof(Response<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubmission(string formId, string id, CancellationToken ct)
    {
        var list = await GetListAsync(SubmissionsConfigKey, ct);
        var index = list.FindIndex(s => s.GetProperty("id").GetString() == id && s.GetProperty("formId").GetString() == formId);
        if (index == -1)
        {
            return NotFound(new Response<object> { StatusCode = StatusCodes.Status404NotFound, Message = "Submission record not found." });
        }

        list.RemoveAt(index);
        await SaveListAsync(SubmissionsConfigKey, list, "Filled digital form submissions logs", ct);

        var response = new Response<object?>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = $"Submission {id} deleted successfully."
        };
        return Ok(response);
    }
}
