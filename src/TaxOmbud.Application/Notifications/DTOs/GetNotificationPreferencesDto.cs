using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Notifications.DTOs;

public record GetNotificationPreferencesQuery();

public record NotificationPreferenceDto(
    string Channel,
    bool Enabled,
    bool EmailEnabled,
    bool SmsEnabled,
    bool InAppEnabled
);
