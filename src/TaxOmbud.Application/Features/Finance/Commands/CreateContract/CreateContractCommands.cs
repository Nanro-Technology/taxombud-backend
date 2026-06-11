using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Finance;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.CreateContract;

public record CreateContractCommands(string ContractNumber, string Title) : IRequest<Result<Guid>>;

public class CreateContractCommandsHandler : IRequestHandler<CreateContractCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateContractCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateContractCommands request, CancellationToken cancellationToken)
    {
        var entity = new Contract
        {
            Id = Guid.NewGuid(),
            ContractNumber = request.ContractNumber,
            Title = request.Title,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        _context.Contracts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}