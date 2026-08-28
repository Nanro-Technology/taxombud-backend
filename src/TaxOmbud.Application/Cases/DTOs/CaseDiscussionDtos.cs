using TaxOmbud.Application.Chats.DTOs;

namespace TaxOmbud.Application.Cases.DTOs;

// ── Commands ────────────────────────────────────────────────────────────────

/// <summary>
/// Auto-called when a case enters Stage 5.
/// No participant list needed — seeded automatically from the Stage 5 workflow role.
/// </summary>
public record OpenCaseDiscussionCommand(Guid CaseId);

/// <summary>
/// Tier 2 override — Admin/Super Admin adds all members of an additional role to a specific case's thread.
/// </summary>
public record AddRoleToDiscussionCommand(
    Guid CaseId,
    string RoleName   // e.g., "Legal", "Audit" — all members of that role are added
);

/// <summary>
/// Tier 2 override — Admin/Super Admin adds a specific individual to a specific case's thread.
/// </summary>
public record AddIndividualParticipantCommand(
    Guid CaseId,
    Guid UserId
);

/// <summary>Remove a Tier 2 individually-added participant from a case's discussion thread.</summary>
public record RemoveDiscussionParticipantCommand(
    Guid CaseId,
    Guid UserId
);

/// <summary>Post a message to a case's investigation discussion thread.</summary>
public record SendDiscussionMessageCommand(
    Guid CaseId,
    string Content,

    /// <summary>
    /// "message" = regular post (default) |
    /// "finding" = formal investigative finding |
    /// "conclusion" = formal conclusion; triggers supervisor notification
    /// </summary>
    string MessageType,

    string? AttachmentUrl,
    string? AttachmentFileName
);

public record PinDiscussionMessageCommand(Guid MessageId, bool IsPinned);

// ── Response DTOs ───────────────────────────────────────────────────────────

/// <summary>Full case discussion thread returned to the client.</summary>
public class CaseDiscussionThreadDto
{
    public Guid ChatId { get; set; }
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = null!;

    /// <summary>True when the case has advanced past Stage 5 — no new posts permitted.</summary>
    public bool IsLocked { get; set; }
    public DateTimeOffset? LockedAt { get; set; }

    /// <summary>The role configured on the workflow for the investigation stage (e.g. "Operations", "Manager").</summary>
    public string? RequiredRoleName { get; set; }
    public Guid? RequiredRoleId { get; set; }

    /// <summary>Whether the current user is authorized to post in this thread based on workflow role assignment.</summary>
    public bool CanCurrentUserPost { get; set; } = true;

    /// <summary>Combined Tier 1 (workflow-role-seeded) and Tier 2 (admin override) participants.</summary>
    public List<DiscussionParticipantDto> Participants { get; set; } = new();
    public List<DiscussionMessageDto> Messages { get; set; } = new();
}

public class DiscussionParticipantDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }

    /// <summary>e.g., "Operations"</summary>
    public string? RoleName { get; set; }

    /// <summary>"role" = Tier 1 (auto-seeded) | "individual" = Tier 2 (admin override) | "stage6" = post-lock read-only</summary>
    public string ParticipantTier { get; set; } = "role";

    public bool IsReadOnly { get; set; }
    public bool IsOnline { get; set; }  // Resolved from ChatHub SignalR presence
}

public class DiscussionMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string? SenderAvatar { get; set; }
    public string? SenderRole { get; set; }
    public string Content { get; set; } = null!;
    public string MessageText => Content;

    /// <summary>"message" | "finding" | "conclusion" | "system"</summary>
    public string MessageType { get; set; } = "message";

    public bool IsPinned { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset SentAt => CreatedAt;
    public List<ReadReceiptDto> ReadReceipts { get; set; } = new();
}

public class SendDiscussionMessageRequest
{
    public string? Content { get; set; }
    public string? MessageText { get; set; }
    public string MessageType { get; set; } = "message";
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }

    public string ResolvedContent => !string.IsNullOrWhiteSpace(Content) ? Content : (MessageText ?? string.Empty);
}
