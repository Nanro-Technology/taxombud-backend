using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Operations;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Operations.Commands.UpdateVendor;

public record UpdateVendorCommand(
    Guid Id,
    string Name, 
    string Company, 
    string Email, 
    string Phone,
    string? Designation,
    string? Scope,
    string? ScopeTarget,
    string? Notes
) : IRequest<Result<Guid>>;

public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public UpdateVendorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VendorContacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(VendorContact), request.Id);
        }

        entity.Name = request.Name;
        entity.Company = request.Company;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Designation = request.Designation;
        entity.Scope = request.Scope;
        entity.ScopeTarget = request.ScopeTarget;
        entity.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
