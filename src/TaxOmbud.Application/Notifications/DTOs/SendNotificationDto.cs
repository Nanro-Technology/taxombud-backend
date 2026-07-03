using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Notifications.DTOs;

public record SendNotificationCommand(Guid UserId, string Title, string Message) ;

public record SentNotificationResponse(
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt
);

public record SendNotificationRequest(Guid UserId, string Title, string Message);
