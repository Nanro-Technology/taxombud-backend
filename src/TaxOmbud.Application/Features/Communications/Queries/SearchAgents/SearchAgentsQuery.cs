using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Communications.DTOs;

namespace TaxOmbud.Application.Features.Communications.Queries.SearchAgents;

public record SearchAgentsQuery(string SearchTerm) : IRequest<List<AgentSummaryDto>>;

public class SearchAgentsQueryHandler : IRequestHandler<SearchAgentsQuery, List<AgentSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchAgentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgentSummaryDto>> Handle(SearchAgentsQuery request, CancellationToken cancellationToken)
    {
        var term = request.SearchTerm?.ToLower() ?? "";

        // We fetch from Users and conditionally join StaffProfiles
        var agents = await _context.Users
            .Where(u => !u.IsDeleted && 
                        (string.IsNullOrEmpty(term) || 
                         u.FirstName.ToLower().Contains(term) || 
                         u.LastName.ToLower().Contains(term) || 
                         u.Email.ToLower().Contains(term)))
            .Select(u => new AgentSummaryDto
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                Role = _context.StaffProfiles.Where(sp => sp.UserId == u.Id && !sp.IsDeleted).Select(sp => sp.Title).FirstOrDefault()
            })
            .Take(50)
            .ToListAsync(cancellationToken);

        return agents;
    }
}
