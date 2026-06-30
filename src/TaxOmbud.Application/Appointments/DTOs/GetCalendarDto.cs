using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Appointments.DTOs;

public record GetCalendarQuery(Guid? OfficerId, Guid? TaxpayerId, int Month, int Year) ;

public record CalendarEventDto(Guid Id, string Title, DateTimeOffset StartTime, DateTimeOffset EndTime, string Status);