using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Commands.CreateSalaryProfile;

public record CreateSalaryProfileCommands(Guid StaffId, decimal BaseSalary) : IRequest<Result<Guid>>;

public class CreateSalaryProfileCommandsHandler : IRequestHandler<CreateSalaryProfileCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateSalaryProfileCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateSalaryProfileCommands request, CancellationToken cancellationToken)
    {
        var entity = new SalaryProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.StaffId,
            Basic = request.BaseSalary,
            EffectiveFrom = DateTime.UtcNow
        };
        _context.SalaryProfiles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}