using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Finance.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _financeService;

    public FinanceController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet("quotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQuotes([FromQuery] GetQuotesQueries query, CancellationToken ct)
    {
        var result = await _financeService.GetQuotesAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("contracts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetContracts([FromQuery] GetContractsQueries query, CancellationToken ct)
    {
        var result = await _financeService.GetContractsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInvoices([FromQuery] GetInvoicesQueries query, CancellationToken ct)
    {
        var result = await _financeService.GetInvoicesAsync(query, ct);
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

    [HttpPost("contracts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractCommands command, CancellationToken ct)
    {
        var result = await _financeService.CreateContractAsync(command, ct);
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
}
