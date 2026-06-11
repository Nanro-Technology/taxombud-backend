using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayerById;

namespace TaxOmbud.Application.Features.Taxpayers.Queries.GetCurrentTaxpayer;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetCurrentTaxpayerQuery : IRequest<Result<TaxpayerDetailDto>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetCurrentTaxpayerQueryHandler : IRequestHandler<GetCurrentTaxpayerQuery, Result<TaxpayerDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCurrentTaxpayerQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<TaxpayerDetailDto>> Handle(GetCurrentTaxpayerQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<TaxpayerDetailDto>.Failure("User is not authenticated.");

        var taxpayer = await _context.TaxpayerProfiles
            .Include(t => t.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

        if (taxpayer is null)
            return Result<TaxpayerDetailDto>.NotFound("Taxpayer profile not found.");

        return Result<TaxpayerDetailDto>.Success(new TaxpayerDetailDto(
            taxpayer.Id,
            taxpayer.UserId,
            taxpayer.User!.FirstName,
            taxpayer.User.LastName,
            taxpayer.User.FullName,
            taxpayer.User.Email,
            taxpayer.User.Phone,
            taxpayer.TaxpayerType.ToString(),
            taxpayer.TinNumber,
            taxpayer.Nin,
            taxpayer.Bvn,
            taxpayer.Gender,
            taxpayer.DateOfBirth,
            taxpayer.CompanyName,
            taxpayer.RcNumber,
            taxpayer.Address,
            taxpayer.City,
            taxpayer.State,
            taxpayer.IsVerified,
            taxpayer.CreatedAt,
            taxpayer.UpdatedAt
        ));
    }
}
