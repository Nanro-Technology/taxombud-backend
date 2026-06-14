using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.CreateProject;

public record CreateProjectCommands(string Name, string Description) : IRequest<Result<Guid>>;

public class CreateProjectCommandsHandler : IRequestHandler<CreateProjectCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateProjectCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateProjectCommands request, CancellationToken cancellationToken)
    {
        var entity = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}