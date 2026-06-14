using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Operations;

namespace TaxOmbud.Application.Features.Operations.Queries.GetVendorById;

public record GetVendorByIdQuery(Guid Id) : IRequest<Result<VendorContact>>;

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, Result<VendorContact>>
{
    private readonly IApplicationDbContext _context;

    public GetVendorByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<VendorContact>> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.VendorContacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(VendorContact), request.Id);
        }

        return Result<VendorContact>.Success(entity);
    }
}
