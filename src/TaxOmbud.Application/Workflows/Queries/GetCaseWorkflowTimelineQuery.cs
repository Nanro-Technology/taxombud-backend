using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;

namespace TaxOmbud.Application.Workflows.Queries;

public record GetCaseWorkflowTimelineQuery(Guid CaseId) : IRequest<List<CaseWorkflowAuditLogDto>>;

public class GetCaseWorkflowTimelineQueryHandler : IRequestHandler<GetCaseWorkflowTimelineQuery, List<CaseWorkflowAuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCaseWorkflowTimelineQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CaseWorkflowAuditLogDto>> Handle(GetCaseWorkflowTimelineQuery request, CancellationToken cancellationToken)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == request.CaseId || c.ComplaintId == request.CaseId, cancellationToken);
        var targetCaseId = caseItem?.Id ?? request.CaseId;

        var logs = await _context.CaseWorkflowAuditLogs
            .Include(l => l.PerformedByUser)
            .AsNoTracking()
            .Where(l => l.CaseId == targetCaseId || l.CaseId == request.CaseId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);

        return logs.Select(l => new CaseWorkflowAuditLogDto(
            l.Id,
            l.CaseId,
            l.PerformedByUserId,
            l.PerformedByUser != null ? $"{l.PerformedByUser.FirstName} {l.PerformedByUser.LastName}" : "System",
            l.UserRole,
            l.Action,
            l.PreviousStatus,
            l.NewStatus,
            l.LevelNumber,
            l.LevelName,
            l.Comment,
            l.Timestamp
        )).ToList();
    }
}
