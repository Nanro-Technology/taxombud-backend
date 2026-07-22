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
    int? SlaHours,
    int? EscalationHours,
    bool IsMandatory,
    bool RequireComment,
    bool RequireAttachment,
    AssignmentTargetType TargetType,
    Guid? TargetRoleId,
    string? TargetRoleName,
    Guid? TargetUserId,
    string? TargetUserName,
    AssignmentMode AssignmentMode,
    AssignmentAlgorithm AssignmentAlgorithm
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

public record CreateWorkflowLevelRequest(
    int LevelNumber,
    string Name,
    string? Description,
    int? SlaHours,
    int? EscalationHours,
    bool IsMandatory,
    bool RequireComment,
    bool RequireAttachment,
    AssignmentTargetType TargetType,
    Guid? TargetRoleId,
    Guid? TargetUserId,
    AssignmentMode AssignmentMode,
    AssignmentAlgorithm AssignmentAlgorithm
);
