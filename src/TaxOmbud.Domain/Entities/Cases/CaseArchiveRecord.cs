using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Cases;

/// <summary>
/// Created at Stage 7 (Closure & Archiving) to formally record the archiving of a case.
/// </summary>
public class CaseArchiveRecord : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    /// <summary>Purpose or reason for archiving (e.g., "Case Resolved", "Not Admissible", "Withdrawn").</summary>
    public string ArchivePurpose { get; set; } = null!;

    /// <summary>List of document references archived with the case (URLs or identifiers).</summary>
    public List<string> ArchivedDocumentRefs { get; set; } = new();

    public Guid ArchivedByUserId { get; set; }
    public DateTimeOffset ArchivedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? Notes { get; set; }
}
