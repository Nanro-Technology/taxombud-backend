using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Communications.DTOs;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Queries.GetSmsMessageById;

public record GetSmsMessageByIdQuery(Guid Id) : IRequest<SmsMessageDto>;

public class GetSmsMessageByIdQueryHandler : IRequestHandler<GetSmsMessageByIdQuery, SmsMessageDto>
{
    private readonly IApplicationDbContext _context;

    public GetSmsMessageByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SmsMessageDto> Handle(GetSmsMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.SmsMessages.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SmsMessage), request.Id);
        }

        return new SmsMessageDto
        {
            Id = entity.Id,
            Provider = entity.Provider,
            SenderId = entity.SenderId,
            Body = entity.Body,
            ScheduledAt = entity.ScheduledAt,
            RecipientType = entity.RecipientType,
            PhoneNumbers = entity.PhoneNumbers,
            Mode = entity.Mode,
            Status = entity.Status,
            Direction = entity.Direction,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }
}
