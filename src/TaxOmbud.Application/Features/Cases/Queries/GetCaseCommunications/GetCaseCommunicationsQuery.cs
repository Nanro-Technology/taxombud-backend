using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Cases.Queries.GetCaseCommunications;

public record GetCaseCommunicationsQuery(Guid CaseId) : IRequest<Result<IReadOnlyList<CaseCommunicationDto>>>;

public record CaseCommunicationDto(
    Guid Id,
    Guid CaseId,
    string Sender,
    string Recipient,
    string Direction,
    string Subject,
    string Body,
    DateTimeOffset SentAt,
    string Channel
);

public class GetCaseCommunicationsQueryHandler : IRequestHandler<GetCaseCommunicationsQuery, Result<IReadOnlyList<CaseCommunicationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCaseCommunicationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CaseCommunicationDto>>> Handle(
        GetCaseCommunicationsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Cases.AnyAsync(c => c.Id == request.CaseId, cancellationToken);
        if (!exists)
            return Result<IReadOnlyList<CaseCommunicationDto>>.NotFound($"Case '{request.CaseId}' was not found.");

        var communications = await _context.CaseCommunicationLogs
            .AsNoTracking()
            .Where(c => c.CaseId == request.CaseId)
            .OrderByDescending(c => c.SentAt)
            .Select(c => new CaseCommunicationDto(
                c.Id,
                c.CaseId,
                c.Sender,
                c.Recipient,
                c.Direction.ToString(),
                c.Subject,
                c.Body,
                c.SentAt,
                c.Channel
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CaseCommunicationDto>>.Success(communications.AsReadOnly());
    }
}
