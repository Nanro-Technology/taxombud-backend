using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.DTOs;

public record WorkflowDto(
    Guid Id,
    string Name,
    string Description,
    string CaseCategory,
    bool IsActive,
    bool IsDefault,
    int CurrentVersion,
    DateTime CreatedAt,
    List<WorkflowLevelDto> Levels
);

public record WorkflowLevelDto(
    Guid Id,
    int LevelNumber,
    string Name,
    string? Description,
    LevelRole LevelRole,
    int? SlaHours,
    int? EscalationHours,
    bool IsMandatory,
    bool RequireComment,
    bool RequireAttachment,
    AssignmentMode AssignmentMode,
    AssignmentAlgorithm AssignmentAlgorithm,
    List<WorkflowLevelTargetDto> Targets
);

/// <summary>Represents one assignment target entry for a workflow level.</summary>
public record WorkflowLevelTargetDto(
    Guid Id,
    WorkflowLevelTargetType TargetType,
    Guid TargetId,
    string TargetName   // Display name resolved from Role/Department/User
);

public record WorkflowVersionDto(
    Guid Id,
    Guid WorkflowId,
    int VersionNumber,
    bool IsPublished,
    DateTimeOffset? PublishedAt
);

public record WorkflowInstanceDto(
    Guid Id,
    Guid CaseId,
    Guid WorkflowId,
    string WorkflowName,
    int CurrentLevelNumber,
    WorkflowStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    List<CaseApprovalTaskDto> ApprovalTasks
);

public record CaseApprovalTaskDto(
    Guid Id,
    Guid WorkflowInstanceId,
    Guid WorkflowInstanceLevelId,
    Guid CaseId,
    string CaseSubject,
    Guid AssignedUserId,
    string AssignedUserName,
    Guid? AssignedRoleId,
    string? AssignedRoleName,
    WorkflowAction Action,
    WorkflowLevelStatus TaskStatus,
    string? Comment,
    DateTimeOffset? PerformedAt,
    DateTime CreatedAt
);

public record CaseWorkflowAuditLogDto(
    Guid Id,
    Guid CaseId,
    Guid PerformedByUserId,
    string PerformedByUserName,
    string UserRole,
    string Action,
    string PreviousStatus,
    string NewStatus,
    int LevelNumber,
    string LevelName,
    string? Comment,
    DateTimeOffset Timestamp
);

/// <summary>Request model for one level when creating or updating a workflow.</summary>
public record CreateWorkflowLevelRequest(
    int LevelNumber,
    string Name,
    string? Description,
    LevelRole LevelRole,
    int? SlaHours,
    int? EscalationHours,
    bool IsMandatory,
    bool RequireComment,
    bool RequireAttachment,
    AssignmentMode AssignmentMode,
    AssignmentAlgorithm AssignmentAlgorithm,
    List<CreateWorkflowLevelTargetRequest>? Targets = null
);

/// <summary>One target entry sent in the workflow level creation/update request.</summary>
public record CreateWorkflowLevelTargetRequest(
    WorkflowLevelTargetType TargetType,
    Guid TargetId
);

/// <summary>DTO returned from the Stage Library API.</summary>
public record WorkflowStageLibraryItemDto(
    Guid Id,
    string Name,
    string? Description,
    LevelRole LevelRole,
    int? DefaultSlaHours,
    int? DefaultEscalationHours,
    AssignmentAlgorithm DefaultAlgorithm,
    bool IsSystemStage,
    int OrderHint
);
