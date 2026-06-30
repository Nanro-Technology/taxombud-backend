using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Webhooks.DTOs;

public record GetWebhookByIdQuery(Guid Id) ;

public record WebhookDetailDto(
    Guid Id,
    string Url,
    string EventTypes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);