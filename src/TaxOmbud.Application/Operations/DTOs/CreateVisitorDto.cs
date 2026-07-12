using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record CreateVisitorCommands(
    string Name,
    string? Email,
    string? Phone,
    string VisitorCode,
    Guid HostId,
    DateTime ExpectedArrival,
    string Status,
    Guid? RequestedById
);
