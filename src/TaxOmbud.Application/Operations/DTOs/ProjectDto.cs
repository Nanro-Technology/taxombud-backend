using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Operations.DTOs;

public record ProjectMemberDto(
    Guid UserId,
    string Name,
    string Initials,
    string Color
);

public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    Guid? OwnerId,
    string OwnerName,
    string StartDate, // Format: yyyy-MM-dd
    string DueDate,   // Format: yyyy-MM-dd
    int Progress,
    string Priority,
    List<ProjectMemberDto> Members,
    int TasksTotal,
    int TasksDone
);
