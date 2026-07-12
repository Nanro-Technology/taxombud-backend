using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Operations.DTOs;

public record UpdateProjectCommand(
    Guid Id,
    string Name,
    string Description,
    string Status,
    DateTime? StartDate,
    DateTime? DueDate,
    Guid? OwnerId,
    List<Guid> MemberIds,
    string Priority,
    int Progress
);
