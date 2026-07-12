using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Knowledge;

/// <summary>
/// A category grouping related knowledge base topics.
/// </summary>
public class KnowledgeCategory : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Description { get; set; } = string.Empty;

    public ICollection<KnowledgeTopic> Topics { get; set; } = new List<KnowledgeTopic>();
}

/// <summary>
/// A specific article / topic within a knowledge category.
/// </summary>
public class KnowledgeTopic : BaseEntity
{
    public Guid CategoryId { get; set; }
    public KnowledgeCategory Category { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;

    /// <summary>JSON array of tag strings (e.g. ["filing", "vat"])</summary>
    public string? TagsJson { get; set; }
}
