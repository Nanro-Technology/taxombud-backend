using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.Wallet.Queries.GetWalletBalance;
using TaxOmbud.Application.Features.Wallet.Queries.GetWalletTransactions;
using TaxOmbud.Application.Features.Wallet.Commands.RequestWithdrawal;
using TaxOmbud.Application.Features.Wallet.Commands.ProcessWithdrawal;

namespace TaxOmbud.Api.Controllers;

public class WalletController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public WalletController(IMediator mediator) { _mediator = mediator; }
    [HttpGet("balance")]
    public async Task<IActionResult> GetWalletBalance([FromQuery] GetWalletBalanceQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet("transactions")]
    public async Task<IActionResult> GetWalletTransactions([FromQuery] GetWalletTransactionsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost("withdrawals/request")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] RequestWithdrawalCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("withdrawals/process")]
    public async Task<IActionResult> ProcessWithdrawal([FromBody] ProcessWithdrawalCommands command) => ToActionResult(await _mediator.Send(command));
}

