using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayerById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetTaxpayerByIdQuery(Guid Id) : IRequest<Result<TaxpayerDetailDto>>;

public record TaxpayerDetailDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string TaxpayerType,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? Gender,
    DateTimeOffset? DateOfBirth,
    string? CompanyName,
    string? RcNumber,
    string? Address,
    string? City,
    string? State,
    bool IsVerified,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetTaxpayerByIdQueryHandler : IRequestHandler<GetTaxpayerByIdQuery, Result<TaxpayerDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTaxpayerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TaxpayerDetailDto>> Handle(GetTaxpayerByIdQuery request, CancellationToken cancellationToken)
    {
        var taxpayer = await _context.TaxpayerProfiles
            .Include(t => t.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (taxpayer == null)
            return Result<TaxpayerDetailDto>.NotFound("Taxpayer profile not found.");

        var dto = new TaxpayerDetailDto(
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
        );

        return Result<TaxpayerDetailDto>.Success(dto);
    }
}
