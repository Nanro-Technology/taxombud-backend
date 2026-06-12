using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.AddVendor;

public record AddVendorCommands(
    string Name, 
    string Company, 
    string Email, 
    string Phone,
    string? Designation,
    string? Scope,
    string? ScopeTarget,
    string? Notes
) : IRequest<Result<Guid>>;

public class AddVendorCommandsHandler : IRequestHandler<AddVendorCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public AddVendorCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(AddVendorCommands request, CancellationToken cancellationToken)
    {
        var entity = new VendorContact
        {
            Name = request.Name,
            Company = request.Company,
            Email = request.Email,
            Phone = request.Phone,
            Designation = request.Designation,
            Scope = request.Scope,
            ScopeTarget = request.ScopeTarget,
            Notes = request.Notes
        };
        _context.VendorContacts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}