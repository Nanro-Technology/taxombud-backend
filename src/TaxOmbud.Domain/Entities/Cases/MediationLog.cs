using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

public class MediationLog : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public DateTimeOffset SessionDate { get; set; }
    public string Attendees { get; set; } = null!;
    public string SummaryOfDiscussions { get; set; } = null!;
    public string? SettlementProposal { get; set; }
    public bool IsAmicablySettled { get; set; }
    public string? AgreementDocumentUrl { get; set; }

    public Guid LoggedByUserId { get; set; }
    public DateTimeOffset LoggedAt { get; set; } = DateTimeOffset.UtcNow;
}
