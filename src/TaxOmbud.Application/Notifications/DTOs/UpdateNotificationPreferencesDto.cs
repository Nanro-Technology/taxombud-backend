using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Notifications;

namespace TaxOmbud.Application.Notifications.DTOs;

public record PreferenceUpdateDto(string Type, bool Email, bool Sms, bool InApp);
public record UpdateNotificationPreferencesCommand(List<PreferenceUpdateDto> Preferences);

public record UpdateNotificationPreferencesRequest(List<PreferenceUpdateDto> Preferences);
