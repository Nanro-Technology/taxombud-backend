using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Operations;

namespace TaxOmbud.Application.Features.Operations.Commands.DeleteVendor;

public record DeleteVendorCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteVendorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VendorContacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(VendorContact), request.Id);
        }

        _context.VendorContacts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
