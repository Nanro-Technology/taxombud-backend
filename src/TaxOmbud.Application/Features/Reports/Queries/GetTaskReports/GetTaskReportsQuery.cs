using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Reports.DTOs;

namespace TaxOmbud.Application.Features.Reports.Queries.GetTaskReports;

public class GetTaskReportsQuery : ReportFilterDto, IRequest<TaskReportDto> { }

public class GetTaskReportsQueryHandler : IRequestHandler<GetTaskReportsQuery, TaskReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetTaskReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskReportDto> Handle(GetTaskReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CaseTasks.AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(t => t.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(t => t.CreatedAt <= request.EndDate.Value);

        var totalTasks = await query.CountAsync(cancellationToken);
        
        var statuses = await query.GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var priorities = await query.GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var completed = statuses.FirstOrDefault(s => s.Status == "Completed")?.Count ?? 0;
        var pending = totalTasks - completed;
        
        var now = DateTimeOffset.UtcNow;
        var overdueTasks = await query.CountAsync(t => t.DueAt < now && t.Status != "Completed", cancellationToken);

        return new TaskReportDto
        {
            TotalTasks = totalTasks,
            CompletedTasks = completed,
            PendingTasks = pending,
            OverdueTasks = overdueTasks,
            TasksByStatus = statuses.ToDictionary(k => k.Status ?? "Unknown", v => v.Count),
            TasksByPriority = priorities.ToDictionary(k => k.Priority ?? "Unknown", v => v.Count)
        };
    }
}
