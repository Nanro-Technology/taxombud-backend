using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Cases.DTOs;

namespace TaxOmbud.Application.Interfaces.Services;

/// <summary>
/// Manages the Investigation-Stage Case Discussion Thread (Stage 5 only).
/// Threads are auto-created when a case enters Stage 5, seeded with workflow-role participants,
/// and locked (read-only) when the case advances to Stage 6.
/// </summary>
public interface ICaseDiscussionService
{
    /// <summary>
    /// Auto-called when a case enters Stage 5.
    /// Creates a new AgentChat bound to the case and seeds Tier 1 participants
    /// from the workflow engine's Stage 5 role assignment.
    /// </summary>
    Task<Guid> OpenDiscussionThreadAsync(Guid caseId, CancellationToken ct = default);

    /// <summary>Returns the full discussion thread for a case, including messages and participants.</summary>
    Task<CaseDiscussionThreadDto?> GetDiscussionThreadAsync(Guid caseId, Guid? currentUserId = null, CancellationToken ct = default);

    /// <summary>
    /// Posts a message to the discussion thread.
    /// Validates that the sender is an active participant and the thread is not locked.
    /// If MessageType == "conclusion", dispatches a notification to the supervisor.
    /// </summary>
    Task<DiscussionMessageDto?> SendMessageAsync(SendDiscussionMessageCommand cmd, Guid senderId, CancellationToken ct = default);

    /// <summary>Pins or unpins a message in the thread. Admin/Super Admin only.</summary>
    Task<bool> PinMessageAsync(PinDiscussionMessageCommand cmd, Guid requestedBy, CancellationToken ct = default);

    /// <summary>
    /// Tier 2 — Adds all members of a named role to a specific case's thread.
    /// Admin/Super Admin only.
    /// </summary>
    Task<bool> AddRoleToThreadAsync(AddRoleToDiscussionCommand cmd, Guid requestedBy, CancellationToken ct = default);

    /// <summary>
    /// Tier 2 — Adds a specific individual to a specific case's thread.
    /// Admin/Super Admin only.
    /// </summary>
    Task<bool> AddIndividualToThreadAsync(AddIndividualParticipantCommand cmd, Guid requestedBy, CancellationToken ct = default);

    /// <summary>Remove a Tier 2 individually-added participant. Admin/Super Admin only.</summary>
    Task<bool> RemoveParticipantAsync(RemoveDiscussionParticipantCommand cmd, Guid requestedBy, CancellationToken ct = default);

    /// <summary>
    /// Locks the discussion thread when the case advances past Stage 5.
    /// Called automatically by the workflow engine when Stage 6 begins.
    /// Tier 1 participants become read-only; Stage 6 role members are added as read-only.
    /// </summary>
    Task<bool> LockDiscussionThreadAsync(Guid caseId, CancellationToken ct = default);
}
