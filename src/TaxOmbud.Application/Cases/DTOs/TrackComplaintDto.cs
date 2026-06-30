using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record TrackComplaintQuery(string TrackingNumber);

public record TrackComplaintResponse(
    string TrackingNumber,
    string Status,
    string CurrentStage,
    string Description,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? UpdatedAt
);