using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Features.Cases.Queries.GetCases;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Queries.GetMyCases;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetMyCasesQuery(
    string? Search,
    string? Stage,
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<CaseListDto>>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetMyCasesQueryHandler : IRequestHandler<GetMyCasesQuery, Result<PagedResult<CaseListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetMyCasesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<CaseListDto>>> Handle(GetMyCasesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId is null)
        {
            return Result<PagedResult<CaseListDto>>.Failure("User is not authenticated.");
        }

        var query = _context.Cases
            .Include(c => c.Complaint)
                .ThenInclude(co => co.Taxpayer!)
            .Include(c => c.AssignedOfficer!)
                .ThenInclude(o => o.User)
            .Where(c => c.AssignedOfficer != null && c.AssignedOfficer.UserId == currentUserId.Value)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(c =>
                c.CaseNumber.Value.Contains(searchLower) ||
                c.Subject.ToLower().Contains(searchLower) ||
                c.Complaint.ReferenceNumber.Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(request.Stage))
        {
            query = query.Where(c => c.CurrentStage == request.Stage.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<CaseStatus>(request.Status, true, out var caseStatus))
        {
            query = query.Where(c => c.Status == caseStatus);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CaseListDto(
                c.Id,
                c.CaseNumber.Value,
                c.ComplaintId,
                c.Complaint.ReferenceNumber,
                c.Complaint.Taxpayer != null ? c.Complaint.Taxpayer.FirstName + " " + c.Complaint.Taxpayer.LastName : "Unknown",
                c.Subject,
                c.Priority,
                c.Status.ToString(),
                c.CurrentStage,
                c.AssignedOfficer != null && c.AssignedOfficer.User != null ? c.AssignedOfficer.User.FullName : "Unassigned",
                c.DueDate,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<CaseListDto>>.Success(new PagedResult<CaseListDto>(items, total, request.Page, request.PageSize));
    }
}
