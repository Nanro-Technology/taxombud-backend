using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.AddInventoryItem;

public record AddInventoryItemCommands(
    string Name,
    string Category,
    string Description,
    string SKU,
    Guid? DepartmentId,
    Guid? AssignedUserId,
    string Location,
    string Mode,
    int Quantity,
    string SerialNumber,
    string ImageUrl,
    string Status,
    string Note
) : IRequest<Result<Guid>>;

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
            Category = request.Category,
            Description = request.Description,
            SKU = request.SKU,
            DepartmentId = request.DepartmentId,
            AssignedUserId = request.AssignedUserId,
            Location = request.Location,
            Mode = request.Mode,
            Quantity = request.Quantity,
            SerialNumber = request.SerialNumber,
            ImageUrl = request.ImageUrl,
            Status = request.Status,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };
        _context.InventoryItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}