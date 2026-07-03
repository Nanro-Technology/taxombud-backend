using System.ComponentModel.DataAnnotations;

namespace TaxOmbud.Domain.Common;

public abstract class BaseEntity : ISoftDelete
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? LastModifiedByUserId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
