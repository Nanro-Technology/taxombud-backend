using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Communications.Queries.GetCommunicationById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetCommunicationByIdQuery(Guid Id) : IRequest<Result<CommunicationDetailDto>>;

public record CommunicationDetailDto(
    Guid Id,
    string Channel,
    string Direction,
    string Subject,
    string Body,
    string Recipient,
    string? RecipientName,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    bool IsSent,
    DateTimeOffset? SentAt,
    string? ErrorMessage,
    Guid? SentByUserId,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetCommunicationByIdQueryHandler : IRequestHandler<GetCommunicationByIdQuery, Result<CommunicationDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCommunicationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CommunicationDetailDto>> Handle(GetCommunicationByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _context.CommunicationLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (log == null)
            return Result<CommunicationDetailDto>.NotFound("Communication log not found.");

        var dto = new CommunicationDetailDto(
            log.Id,
            log.Channel,
            log.Direction.ToString(),
            log.Subject,
            log.Body,
            log.Recipient,
            log.RecipientName,
            log.RelatedEntityId,
            log.RelatedEntityType,
            log.IsSent,
            log.SentAt,
            log.ErrorMessage,
            log.SentByUserId,
            log.CreatedAt
        );

        return Result<CommunicationDetailDto>.Success(dto);
    }
}
