using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Communications.DTOs;

public record GetCommunicationsQuery(
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    string? Channel,
    string? Direction,
    int Page = 1,
    int PageSize = 20
) ;

public record CommunicationListDto(
    Guid Id,
    string Channel,
    string Direction,
    string Subject,
    string Recipient,
    string? RecipientName,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    bool IsSent,
    DateTimeOffset? SentAt,
    string? ErrorMessage,
    DateTimeOffset CreatedAt
);
