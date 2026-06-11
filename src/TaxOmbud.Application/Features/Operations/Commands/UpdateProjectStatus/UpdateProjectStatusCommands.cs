using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.UpdateProjectStatus;

public record UpdateProjectStatusCommands(Guid Id, string Status) : IRequest<Result<bool>>;

public class UpdateProjectStatusCommandsHandler : IRequestHandler<UpdateProjectStatusCommands, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public UpdateProjectStatusCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(UpdateProjectStatusCommands request, CancellationToken cancellationToken)
    {
        var entity = await _context.Projects.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if(entity == null) return Result<bool>.NotFound($"Project {request.Id} not found.");
        
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}