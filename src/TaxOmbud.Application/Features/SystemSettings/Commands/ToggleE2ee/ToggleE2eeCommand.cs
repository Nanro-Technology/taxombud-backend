using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Features.SystemSettings.Commands.ToggleE2ee;

public record ToggleE2eeCommand(bool Enable) : IRequest<Result<Unit>>;

public class ToggleE2eeCommandHandler : IRequestHandler<ToggleE2eeCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public ToggleE2eeCommandHandler(IApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<Unit>> Handle(ToggleE2eeCommand request, CancellationToken cancellationToken)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "E2EE_ENABLED", cancellationToken);

        if (setting == null)
        {
            setting = new SystemSetting
            {
                Key = "E2EE_ENABLED",
                Value = request.Enable.ToString(),
                Description = "Globally enables or disables End-to-End Encryption for API payloads."
            };
            _context.SystemSettings.Add(setting);
        }
        else
        {
            setting.Value = request.Enable.ToString();
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        await _cache.RemoveAsync("E2EE_ENABLED", cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
