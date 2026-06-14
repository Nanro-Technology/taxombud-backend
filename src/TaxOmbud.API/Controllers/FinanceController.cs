using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.Finance.Queries.GetQuotes;
using TaxOmbud.Application.Features.Finance.Queries.GetContracts;
using TaxOmbud.Application.Features.Finance.Queries.GetInvoices;
using TaxOmbud.Application.Features.Finance.Commands.CreateQuote;
using TaxOmbud.Application.Features.Finance.Commands.CreateContract;
using TaxOmbud.Application.Features.Finance.Commands.GenerateInvoice;
using TaxOmbud.Application.Features.Finance.Commands.PayInvoice;

namespace TaxOmbud.Api.Controllers;

public class FinanceController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public FinanceController(IMediator mediator) { _mediator = mediator; }

    [HttpGet("quotes")]
    public async Task<IActionResult> GetQuotes([FromQuery] GetQuotesQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet("contracts")]
    public async Task<IActionResult> GetContracts([FromQuery] GetContractsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] GetInvoicesQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost("quotes")]
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("contracts")]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("invoices")]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("invoices/pay")]
    public async Task<IActionResult> PayInvoice([FromBody] PayInvoiceCommands command) => ToActionResult(await _mediator.Send(command));
}