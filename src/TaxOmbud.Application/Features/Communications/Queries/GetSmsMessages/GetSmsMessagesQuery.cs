using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Communications.DTOs;

namespace TaxOmbud.Application.Features.Communications.Queries.GetSmsMessages;

public record GetSmsMessagesQuery : IRequest<List<SmsMessageDto>>;

public class GetSmsMessagesQueryHandler : IRequestHandler<GetSmsMessagesQuery, List<SmsMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSmsMessagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SmsMessageDto>> Handle(GetSmsMessagesQuery request, CancellationToken cancellationToken)
    {
        return await _context.SmsMessages
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SmsMessageDto
            {
                Id = x.Id,
                Provider = x.Provider,
                SenderId = x.SenderId,
                Body = x.Body,
                ScheduledAt = x.ScheduledAt,
                RecipientType = x.RecipientType,
                PhoneNumbers = x.PhoneNumbers,
                Mode = x.Mode,
                Status = x.Status,
                Direction = x.Direction,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            })
            .ToListAsync(cancellationToken);
    }
}
