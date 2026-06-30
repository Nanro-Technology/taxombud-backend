using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Webhooks.DTOs;

public record CreateWebhookCommand(
    string Url,
    string Secret,
    string[] EventTypes
) ;

public record CreatedWebhookResponse(
    Guid Id,
    string Url,
    string EventTypes,
    bool IsActive
);

public record CreateWebhookRequest(string Url, string Secret, string[] EventTypes);