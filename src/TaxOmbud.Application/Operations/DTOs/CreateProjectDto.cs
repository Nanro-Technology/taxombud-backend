using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Operations.DTOs;

public record CreateProjectCommands(
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
