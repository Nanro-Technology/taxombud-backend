using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Communications;

/// <summary>
/// A single public visitor's chatbot session — persists the full transcript.
/// </summary>
public class ChatbotSession : BaseEntity
{
    /// <summary>Display name or "Anonymous"</summary>
    public string VisitorName { get; set; } = "Anonymous";

    public string? VisitorEmail { get; set; }

    /// <summary>Platform origin: Web, WhatsApp, etc.</summary>
    public string Platform { get; set; } = "Web";

    /// <summary>open | handoff | closed</summary>
    public string Status { get; set; } = "open";

    /// <summary>Short preview text (last user message)</summary>
    public string Preview { get; set; } = string.Empty;

    /// <summary>Assigned human agent user ID (when in handoff)</summary>
    public string? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }

    public ICollection<ChatbotMessage> Messages { get; set; } = new List<ChatbotMessage>();
}

/// <summary>
/// A single message within a chatbot session.
/// </summary>
public class ChatbotMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public ChatbotSession Session { get; set; } = null!;

    /// <summary>user | assistant | agent | system</summary>
    public string Sender { get; set; } = null!;

    public string Content { get; set; } = null!;

    /// <summary>JSON array of citation strings surfaced by the AI.</summary>
    public string? CitationsJson { get; set; }

    /// <summary>True if this message triggered or acknowledged a handoff.</summary>
    public bool IsHandoffTrigger { get; set; } = false;
}
