using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Appointments.DTOs;

public record GetAvailabilityQuery(Guid OfficerId, DateTimeOffset Date) ;

public record TimeSlotDto(DateTimeOffset StartTime, DateTimeOffset EndTime, bool IsAvailable);