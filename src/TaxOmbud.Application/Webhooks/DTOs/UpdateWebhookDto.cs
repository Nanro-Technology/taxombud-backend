using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Webhooks.DTOs;

public record UpdateWebhookCommand(
    Guid Id,
    string Url,
    string[] EventTypes,
    bool IsActive
) ;