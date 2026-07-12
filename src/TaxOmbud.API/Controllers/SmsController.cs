using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Communications.DTOs;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/sms")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SmsController : ControllerBase
{
    private readonly ICommunicationsService _communicationsService;

    public SmsController(ICommunicationsService communicationsService)
    {
        _communicationsService = communicationsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSmsMessages(CancellationToken ct)
    {
        var list = await _communicationsService.GetSmsMessagesAsync(new GetSmsMessagesQuery(), ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSmsMessageById(Guid id, CancellationToken ct)
    {
        var item = await _communicationsService.GetSmsMessageByIdAsync(new GetSmsMessageByIdQuery(id), ct);
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSmsMessage([FromBody] CreateSmsMessageCommand command, CancellationToken ct)
    {
        var newId = await _communicationsService.CreateSmsMessageAsync(command, ct);
        return Ok(newId);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSmsMessage(Guid id, [FromBody] UpdateSmsMessageCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in route does not match ID in payload.");
        }
        var updated = await _communicationsService.UpdateSmsMessageAsync(command, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSmsMessage(Guid id, CancellationToken ct)
    {
        var deleted = await _communicationsService.DeleteSmsMessageAsync(new DeleteSmsMessageCommand(id), ct);
        return Ok(deleted);
    }
}
