using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Wallet.DTOs;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService _walletService)
    {
        this._walletService = _walletService;
    }

    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWalletBalance([FromQuery] GetWalletBalanceQueries query, CancellationToken ct)
    {
        var result = await _walletService.GetWalletBalanceAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWalletTransactions([FromQuery] GetWalletTransactionsQueries query, CancellationToken ct)
    {
        var result = await _walletService.GetWalletTransactionsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("withdrawals/request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestWithdrawal([FromBody] RequestWithdrawalCommands command, CancellationToken ct)
    {
        var result = await _walletService.RequestWithdrawalAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("withdrawals/process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessWithdrawal([FromBody] ProcessWithdrawalCommands command, CancellationToken ct)
    {
        var result = await _walletService.ProcessWithdrawalAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}
