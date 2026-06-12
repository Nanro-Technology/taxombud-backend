using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Reports.DTOs;

namespace TaxOmbud.Application.Features.Reports.Queries.GetErpReports;

public class GetErpReportsQuery : ReportFilterDto, IRequest<ErpReportDto> { }

public class GetErpReportsQueryHandler : IRequestHandler<GetErpReportsQuery, ErpReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetErpReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErpReportDto> Handle(GetErpReportsQuery request, CancellationToken cancellationToken)
    {
        var payrollsQuery = _context.PayrollRuns.AsQueryable();

        if (request.StartDate.HasValue)
            payrollsQuery = payrollsQuery.Where(p => p.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            payrollsQuery = payrollsQuery.Where(p => p.CreatedAt <= request.EndDate.Value);

        var totalPayrollRuns = await payrollsQuery.CountAsync(cancellationToken);

        // Calculate this month expense
        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var totalExpenseThisMonth = await _context.PayrollRuns
            .Where(p => p.CreatedAt >= startOfMonth)
            .SumAsync(p => p.TotalNet, cancellationToken);

        var activeContracts = await _context.Contracts.CountAsync(c => c.Status == "Active", cancellationToken);
        var totalQuotes = await _context.Quotes.CountAsync(cancellationToken);

        return new ErpReportDto
        {
            TotalPayrollRuns = totalPayrollRuns,
            TotalPayrollExpenseThisMonth = totalExpenseThisMonth,
            ActiveContracts = activeContracts,
            TotalQuotes = totalQuotes
        };
    }
}
