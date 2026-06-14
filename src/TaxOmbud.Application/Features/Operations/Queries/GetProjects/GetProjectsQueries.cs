using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Queries.GetProjects;

public record GetProjectsQueries : IRequest<Result<List<Project>>> { }

public class GetProjectsQueriesHandler : IRequestHandler<GetProjectsQueries, Result<List<Project>>>
{
    private readonly IApplicationDbContext _context;
    public GetProjectsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<Project>>> Handle(GetProjectsQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.Projects.ToListAsync(cancellationToken);
        return Result<List<Project>>.Success(list);
    }
}