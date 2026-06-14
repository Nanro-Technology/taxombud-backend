using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Reports.DTOs;

namespace TaxOmbud.Application.Features.Reports.Queries.GetAgentReports;

public class GetAgentReportsQuery : ReportFilterDto, IRequest<AgentReportDto> { }

public class GetAgentReportsQueryHandler : IRequestHandler<GetAgentReportsQuery, AgentReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetAgentReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AgentReportDto> Handle(GetAgentReportsQuery request, CancellationToken cancellationToken)
    {
        var casesQuery = _context.Cases.AsQueryable();

        if (request.StartDate.HasValue)
            casesQuery = casesQuery.Where(c => c.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            casesQuery = casesQuery.Where(c => c.CreatedAt <= request.EndDate.Value);

        var agents = await _context.Users
            .Where(u => !u.IsDeleted)
            .ToListAsync(cancellationToken);

        var dto = new AgentReportDto();

        foreach (var agent in agents)
        {
            var agentCases = casesQuery.Where(c => c.AssignedOfficerId == agent.Id);
            
            var assignedCount = await agentCases.CountAsync(cancellationToken);
            var resolvedCount = await agentCases.CountAsync(c => c.Status == TaxOmbud.Domain.Enums.CaseStatus.Closed, cancellationToken);

            var resolvedCases = await agentCases
                .Where(c => c.Status == TaxOmbud.Domain.Enums.CaseStatus.Closed && c.ClosedAt != null)
                .Select(c => new { c.CreatedAt, c.ClosedAt })
                .ToListAsync(cancellationToken);

            double avgResolutionTime = 0;
            if (resolvedCases.Any())
            {
                avgResolutionTime = resolvedCases.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalHours);
            }

            var interactionsCount = await _context.Interactions
                .Where(i => i.LoggedById == agent.Id && (!request.StartDate.HasValue || i.CreatedAt >= request.StartDate) && (!request.EndDate.HasValue || i.CreatedAt <= request.EndDate))
                .CountAsync(cancellationToken);

            if (assignedCount > 0 || interactionsCount > 0)
            {
                dto.AgentPerformances.Add(new AgentPerformanceDto
                {
                    AgentId = agent.Id,
                    AgentName = $"{agent.FirstName} {agent.LastName}",
                    CasesAssigned = assignedCount,
                    CasesResolved = resolvedCount,
                    AverageResolutionTimeHours = avgResolutionTime,
                    InteractionsHandled = interactionsCount
                });
            }
        }

        return dto;
    }
}
