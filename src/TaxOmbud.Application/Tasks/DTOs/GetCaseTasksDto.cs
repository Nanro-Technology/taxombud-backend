using System;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Tasks.DTOs;

// ─── Queries / Commands ────────────────────────────────────────────────────────
public record GetCaseTasksQuery;
public record GetCaseTaskByIdQuery(Guid Id);
public record CreateCaseTaskCommand(string Title, string? Description, string Status, string Priority,
    DateTimeOffset? DueAt, Guid? AssignedToId, Guid? LinkedCaseId);
public record UpdateCaseTaskCommand(Guid Id, string Title, string? Description, string Status, string Priority,
    DateTimeOffset? DueAt, Guid? AssignedToId, Guid? LinkedCaseId);
public record DeleteCaseTaskCommand(Guid Id);

// ─── DTOs ─────────────────────────────────────────────────────────────────────
public class CaseTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTimeOffset? DueAt { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? LinkedCaseId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}