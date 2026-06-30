using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Communications.DTOs;

public record LogCommunicationCommand(
    string Channel,
    string Subject,
    string Body,
    string Recipient,
    string? RecipientName,
    Guid? RelatedEntityId,
    string? RelatedEntityType
) ;

public record LoggedCommunicationResponse(
    Guid Id,
    string Channel,
    string Subject,
    string Recipient,
    bool IsSent,
    DateTimeOffset? SentAt
);

public record LogCommunicationRequest(
    string Channel, string Subject, string Body, string Recipient,
    string? RecipientName, Guid? RelatedEntityId, string? RelatedEntityType
);