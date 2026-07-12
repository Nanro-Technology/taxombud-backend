using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Finance.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/finance")]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _financeService;

    public FinanceController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    // ─── Quotes ───────────────────────────────────────────────────────────────

    [HttpGet("quotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuotes([FromQuery] GetQuotesQueries query, CancellationToken ct)
    {
        var result = await _financeService.GetQuotesAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("quotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteCommands command, CancellationToken ct)
    {
        var result = await _financeService.CreateQuoteAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("quotes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuote(Guid id, [FromBody] UpdateQuoteCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match body ID.");
        }
        var result = await _financeService.UpdateQuoteAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("quotes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuote(Guid id, CancellationToken ct)
    {
        var result = await _financeService.DeleteQuoteAsync(new DeleteQuoteCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Contracts ────────────────────────────────────────────────────────────

    [HttpGet("contracts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContracts([FromQuery] GetContractsQueries query, CancellationToken ct)
    {
        var result = await _financeService.GetContractsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("contracts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractCommands command, CancellationToken ct)
    {
        var result = await _financeService.CreateContractAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("contracts/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContract(Guid id, [FromBody] UpdateContractCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match body ID.");
        }
        var result = await _financeService.UpdateContractAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("contracts/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContract(Guid id, CancellationToken ct)
    {
        var result = await _financeService.DeleteContractAsync(new DeleteContractCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Invoices ─────────────────────────────────────────────────────────────

    [HttpGet("invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices([FromQuery] GetInvoicesQueries query, CancellationToken ct)
    {
        var result = await _financeService.GetInvoicesAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceCommands command, CancellationToken ct)
    {
        var result = await _financeService.GenerateInvoiceAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("invoices/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PayInvoice([FromBody] PayInvoiceCommands command, CancellationToken ct)
    {
        var result = await _financeService.PayInvoiceAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("invoices/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInvoice(Guid id, CancellationToken ct)
    {
        var result = await _financeService.DeleteInvoiceAsync(new DeleteInvoiceCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
