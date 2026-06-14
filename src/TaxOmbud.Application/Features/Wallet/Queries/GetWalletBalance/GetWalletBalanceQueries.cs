using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Wallet.Queries.GetWalletBalance;

public record GetWalletBalanceQueries(Guid UserId) : IRequest<Result<EmployeeWallet>>;

public class GetWalletBalanceQueriesHandler : IRequestHandler<GetWalletBalanceQueries, Result<EmployeeWallet>>
{
    private readonly IApplicationDbContext _context;
    public GetWalletBalanceQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<EmployeeWallet>> Handle(GetWalletBalanceQueries request, CancellationToken cancellationToken)
    {
        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
        if (wallet == null) return Result<EmployeeWallet>.NotFound("Wallet not found.");
        return Result<EmployeeWallet>.Success(wallet);
    }
}