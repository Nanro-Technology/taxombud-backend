using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayerById;
using Mapster;

namespace TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayerByTin;

public record GetTaxpayerByTinQuery(string Tin) : IRequest<Result<TaxpayerDetailDto>>;

public class GetTaxpayerByTinQueryHandler : IRequestHandler<GetTaxpayerByTinQuery, Result<TaxpayerDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTaxpayerByTinQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TaxpayerDetailDto>> Handle(GetTaxpayerByTinQuery request, CancellationToken cancellationToken)
    {
        var taxpayer = await _context.Taxpayers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TaxId != null && t.TaxId.Value == request.Tin, cancellationToken);

        if (taxpayer == null)
            throw new NotFoundException($"Taxpayer with TIN {request.Tin} not found.");

        return Result<TaxpayerDetailDto>.Success(taxpayer.Adapt<TaxpayerDetailDto>());
    }
}
