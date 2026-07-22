using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Events.Workflows;

public record WorkflowCreatedEvent(Guid WorkflowId, string Name, DateTimeOffset OccurredOn) : IDomainEvent;

public record CaseSubmittedToWorkflowEvent(Guid CaseId, Guid WorkflowInstanceId, Guid WorkflowId, DateTimeOffset OccurredOn) : IDomainEvent;

public record CaseApprovalTaskAssignedEvent(Guid CaseId, Guid TaskId, Guid AssignedUserId, int LevelNumber, string LevelName, DateTimeOffset OccurredOn) : IDomainEvent;

public record CaseApprovedEvent(Guid CaseId, Guid TaskId, Guid PerformedByUserId, int LevelNumber, bool IsFinalApproval, DateTimeOffset OccurredOn) : IDomainEvent;

public record CaseRejectedEvent(Guid CaseId, Guid TaskId, Guid PerformedByUserId, string Comment, DateTimeOffset OccurredOn) : IDomainEvent;

public record CaseReturnedEvent(Guid CaseId, Guid TaskId, Guid PerformedByUserId, string Comment, int ReturnedToLevelNumber, DateTimeOffset OccurredOn) : IDomainEvent;

public record CaseEscalatedEvent(Guid CaseId, Guid TaskId, Guid PerformedByUserId, string Reason, DateTimeOffset OccurredOn) : IDomainEvent;

public record CaseReassignedEvent(Guid CaseId, Guid TaskId, Guid PreviousAssigneeId, Guid NewAssigneeId, Guid ReassignedByUserId, string Reason, DateTimeOffset OccurredOn) : IDomainEvent;
