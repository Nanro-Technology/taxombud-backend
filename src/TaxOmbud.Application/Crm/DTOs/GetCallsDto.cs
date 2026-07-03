using System;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Crm.DTOs;

// ─── Queries / Commands ────────────────────────────────────────────────────────
public record GetCallsQuery;
public record GetCallByIdQuery(Guid Id);
public record CreateCallCommand(string Subject, string CallerType, string CallerMethod, string CallerIdentifier,
    string? CalleeMethod, string? CalleeIdentifier, string Direction, string Status, string? Phone,
    string? Notes, Guid? LinkedToId, Guid? AgentId, DateTimeOffset? StartAt, DateTimeOffset? EndAt);
public record UpdateCallCommand(Guid Id, string Subject, string CallerType, string CallerMethod, string CallerIdentifier,
    string? CalleeMethod, string? CalleeIdentifier, string Direction, string Status, string? Phone,
    string? Notes, Guid? LinkedToId, Guid? AgentId, DateTimeOffset? StartAt, DateTimeOffset? EndAt);
public record DeleteCallCommand(Guid Id);

public record GetInteractionsQuery;
public record GetInteractionByIdQuery(Guid Id);
public record CreateInteractionCommand(string Direction, string Subject, string Type, string Channel,
    string? Outcome, string? Notes, Guid? RelatedToId, Guid? LoggedById, DateTimeOffset? OccurredAt);
public record UpdateInteractionCommand(Guid Id, string Direction, string Subject, string Type, string Channel,
    string? Outcome, string? Notes, Guid? RelatedToId, Guid? LoggedById, DateTimeOffset? OccurredAt);
public record DeleteInteractionCommand(Guid Id);

public record GetOrganizationsQuery;
public record GetOrganizationByIdQuery(Guid Id);
public record CreateOrganizationCommand(string Name, string? Phone, string? Email, Guid? PrimaryTaxPayerId);
public record UpdateOrganizationCommand(Guid Id, string Name, string? Phone, string? Email, Guid? PrimaryTaxPayerId);
public record DeleteOrganizationCommand(Guid Id);

// ─── DTOs ─────────────────────────────────────────────────────────────────────
public class CallDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? CallerType { get; set; }
    public string? CallerMethod { get; set; }
    public string? CallerIdentifier { get; set; }
    public string? CalleeMethod { get; set; }
    public string? CalleeIdentifier { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public Guid? LinkedToId { get; set; }
    public Guid? AgentId { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public class InteractionDto
{
    public Guid Id { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
    public Guid? RelatedToId { get; set; }
    public Guid? LoggedById { get; set; }
    public DateTime? OccurredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? PrimaryTaxPayerId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
