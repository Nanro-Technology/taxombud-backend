using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Queries.GetSalaryProfiles;

public record GetSalaryProfilesQueries : IRequest<Result<List<SalaryProfile>>> { }

public class GetSalaryProfilesQueriesHandler : IRequestHandler<GetSalaryProfilesQueries, Result<List<SalaryProfile>>>
{
    private readonly IApplicationDbContext _context;
    public GetSalaryProfilesQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<SalaryProfile>>> Handle(GetSalaryProfilesQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.SalaryProfiles.ToListAsync(cancellationToken);
        return Result<List<SalaryProfile>>.Success(list);
    }
}