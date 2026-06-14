using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Reports.DTOs;

namespace TaxOmbud.Application.Features.Reports.Queries.GetHrReports;

public class GetHrReportsQuery : ReportFilterDto, IRequest<HrReportDto> { }

public class GetHrReportsQueryHandler : IRequestHandler<GetHrReportsQuery, HrReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetHrReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HrReportDto> Handle(GetHrReportsQuery request, CancellationToken cancellationToken)
    {
        var totalEmployees = await _context.StaffProfiles.CountAsync(s => s.EmploymentStatus == "Active", cancellationToken);
        
        var now = DateTimeOffset.UtcNow;
        var activeLeaves = await _context.LeaveRequests.CountAsync(l => 
            l.Status == "Approved" && 
            l.StartDate <= now && 
            l.EndDate >= now, cancellationToken);

        var pendingDisciplinary = await _context.DisciplinaryCases.CountAsync(d => d.Status == "Open" || d.Status == "In Progress", cancellationToken);

        var attendanceQuery = _context.AttendanceLogs.AsQueryable();
        if (request.StartDate.HasValue)
            attendanceQuery = attendanceQuery.Where(a => a.Date >= request.StartDate.Value);
        if (request.EndDate.HasValue)
            attendanceQuery = attendanceQuery.Where(a => a.Date <= request.EndDate.Value);

        var attendanceStatus = await attendanceQuery.GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var departments = await _context.StaffProfiles
            .Include(s => s.User)
            .Where(s => s.EmploymentStatus == "Active")
            .GroupBy(s => s.User.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new HrReportDto
        {
            TotalEmployees = totalEmployees,
            ActiveLeaves = activeLeaves,
            PendingDisciplinaryCases = pendingDisciplinary,
            AttendanceStatusToday = attendanceStatus.ToDictionary(k => k.Status ?? "Unknown", v => v.Count),
            EmployeesByDepartment = departments.ToDictionary(k => k.DepartmentId?.ToString() ?? "Unknown", v => v.Count)
        };
    }
}
