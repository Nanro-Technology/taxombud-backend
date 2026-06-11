using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Officers.Queries.GetOfficerPerformance;

public record GetOfficerPerformanceQuery(Guid OfficerId) : IRequest<Result<OfficerPerformanceDto>>;

public record OfficerPerformanceDto(
    Guid OfficerId,
    int CasesAssigned,
    int CasesResolved,
    int CasesOverdue,
    double AverageResolutionTimeDays
);

public class GetOfficerPerformanceQueryHandler : IRequestHandler<GetOfficerPerformanceQuery, Result<OfficerPerformanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOfficerPerformanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OfficerPerformanceDto>> Handle(GetOfficerPerformanceQuery request, CancellationToken cancellationToken)
    {
        var officerExists = await _context.Users.AnyAsync(u => u.Id == request.OfficerId, cancellationToken);
        if (!officerExists)
            throw new NotFoundException($"Officer with ID {request.OfficerId} not found.");

        var officerCases = await _context.Cases
            .Where(c => c.AssignedOfficerId == request.OfficerId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalAssigned = officerCases.Count;
        var totalResolved = officerCases.Count(c => c.Status == CaseStatus.Closed);
        var overdue = officerCases.Count(c => c.DueDate.HasValue && c.DueDate.Value < DateTime.UtcNow && c.Status != CaseStatus.Closed);

        var resolvedCases = officerCases.Where(c => c.Status == CaseStatus.Closed && c.ClosedAt.HasValue).ToList();
        double avgResolution = 0;
        if (resolvedCases.Any())
        {
            avgResolution = resolvedCases.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalDays);
        }

        var dto = new OfficerPerformanceDto(
            request.OfficerId,
            totalAssigned,
            totalResolved,
            overdue,
            Math.Round(avgResolution, 1)
        );

        return Result<OfficerPerformanceDto>.Success(dto);
    }
}
