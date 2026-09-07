using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Cases;

namespace TaxOmbud.Domain.Entities.Communications;

public class AgentChat : BaseEntity
{
    public string? Topic { get; set; }
    public bool IsGroupChat { get; set; } = false;

    // ── Participant tracking (two modes) ────────────────────────────────────
    // Legacy general chats: ParticipantIds = JSON string of Guid[] (used by ChatsService).
    // Case discussion threads: Participants = junction entity collection (used by CaseDiscussionService).

    /// <summary>JSON-encoded list of participant user IDs. Used by general agent-to-agent chats.</summary>
    public string ParticipantIds { get; set; } = "[]";

    /// <summary>Structured participant records. Used exclusively by case discussion threads (BoundToStage is set).</summary>
    public ICollection<AgentChatParticipant> Participants { get; set; } = new List<AgentChatParticipant>();

    // ── Case Discussion Thread Binding ──────────────────────────────────────
    // Null = general agent-to-agent chat. Set = Investigation-stage case discussion thread.

    /// <summary>FK to the case this thread is bound to. Null for general chats.</summary>
    public Guid? CaseId { get; set; }
    public Case? Case { get; set; }

    /// <summary>
    /// Workflow stage this thread is bound to.
    /// For case discussion threads this is WorkflowStage.InvestigationAndResolution ("5_investigation").
    /// </summary>
    public string? BoundToStage { get; set; }

    /// <summary>
    /// Set to true when the case advances beyond Stage 5 (enters Stage 6 Decision & Communication).
    /// Once locked, no new messages can be posted. Stage 6 role members gain read-only access.
    /// </summary>
    public bool IsLocked { get; set; } = false;

    public DateTimeOffset? LockedAt { get; set; }
    // ────────────────────────────────────────────────────────────────────────

    public ICollection<AgentChatMessage> Messages { get; set; } = new List<AgentChatMessage>();
}

/// <summary>
/// Junction entity for AgentChat participants.
/// Tracks who is in a chat, the role they were added under, their participation tier,
/// and whether they have read-only access (post-lock).
/// Replaces the old JSON ParticipantIds string on AgentChat.
/// </summary>
public class AgentChatParticipant : BaseEntity
{
    public Guid AgentChatId { get; set; }
    public AgentChat Chat { get; set; } = null!;

    public Guid UserId { get; set; }

    /// <summary>
    /// The role name this participant was enrolled under (e.g., "Operations").
    /// Null for Tier 2 individual overrides added without a specific role context.
    /// </summary>
    public string? RoleName { get; set; }

    /// <summary>
    /// "role" = auto-seeded from the workflow engine's Stage 5 role assignment (Tier 1).
    /// "individual" = manually added by Admin/Super Admin for a specific case (Tier 2).
    /// "stage6" = read-only access granted automatically when the thread locks (Stage 6 role members).
    /// </summary>
    public string ParticipantTier { get; set; } = "role";

    /// <summary>
    /// True for Stage 6 role members who can read the locked thread but cannot post.
    /// </summary>
    public bool IsReadOnly { get; set; } = false;

    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class AgentChatMessage : BaseEntity
{
    public Guid AgentChatId { get; set; }
    public AgentChat Chat { get; set; } = null!;

    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;

    /// <summary>
    /// Message type determines display and system behaviour:
    /// "message"    — regular discussion post (default)
    /// "finding"    — officer has documented a formal investigative finding
    /// "conclusion" — officer declares a conclusion; triggers supervisor notification
    /// "system"     — auto-generated (e.g., "Thread opened", "Participant added", "Thread locked")
    /// </summary>
    public string MessageType { get; set; } = "message";

    public bool IsPinned { get; set; } = false;

    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }

    // Read receipts stored as JSON: [{ "UserId": "guid", "ReadAt": "datetime" }]
    public string ReadReceipts { get; set; } = "[]";
}
