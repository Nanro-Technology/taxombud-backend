using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record GetVisitorsQueries(
    string? Status = null,
    DateTime? ExpectedDateFrom = null,
    DateTime? ExpectedDateTo = null,
    Guid? HostId = null
);
