using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Communications.DTOs;

public record GetCommunicationByIdQuery(Guid Id) ;

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