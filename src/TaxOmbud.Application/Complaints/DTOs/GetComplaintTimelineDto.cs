using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record GetComplaintTimelineQuery(Guid ComplaintId) ;

public record TimelineEventDto(
    string EventType,
    string Description,
    string? OldStatus,
    string? NewStatus,
    string? ChangedBy,
    DateTimeOffset OccurredAt
);