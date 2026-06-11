using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.AddInventoryItem;

public record AddInventoryItemCommands(string Name, string SKU, int Quantity) : IRequest<Result<Guid>>;

public class AddInventoryItemCommandsHandler : IRequestHandler<AddInventoryItemCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public AddInventoryItemCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(AddInventoryItemCommands request, CancellationToken cancellationToken)
    {
        var entity = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SKU = request.SKU,
            Quantity = request.Quantity,
            CreatedAt = DateTime.UtcNow
        };
        _context.InventoryItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}