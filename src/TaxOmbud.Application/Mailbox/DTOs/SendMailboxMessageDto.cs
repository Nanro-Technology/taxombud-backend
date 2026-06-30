namespace TaxOmbud.Application.Mailbox.DTOs;

public record SendMailboxMessageRequest(string Subject, string BodyText, bool IsDraft, List<Guid> ToRecipients);
