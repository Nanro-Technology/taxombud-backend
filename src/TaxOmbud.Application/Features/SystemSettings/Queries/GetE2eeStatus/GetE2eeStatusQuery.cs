using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.SystemSettings.Queries.GetE2eeStatus;

public record GetE2eeStatusQuery() : IRequest<Result<E2eeStatusDto>>;

public record E2eeStatusDto(bool IsEnabled);

public class GetE2eeStatusQueryHandler : IRequestHandler<GetE2eeStatusQuery, Result<E2eeStatusDto>>
{
    private readonly IApplicationDbContext _context;

    public GetE2eeStatusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<E2eeStatusDto>> Handle(GetE2eeStatusQuery request, CancellationToken cancellationToken)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "E2EE_ENABLED", cancellationToken);

        bool isEnabled = false;
        if (setting != null && bool.TryParse(setting.Value, out var val))
        {
            isEnabled = val;
        }

        return Result<E2eeStatusDto>.Success(new E2eeStatusDto(isEnabled));
    }
}
