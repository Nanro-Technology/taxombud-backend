using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Communications;

public class CommunicationTemplate : BaseEntity
{
    public string Name { get; set; } = null!; // unique identifier key
    public string SubjectTemplate { get; set; } = null!;
    public string BodyTemplate { get; set; } = null!;
    public string Channel { get; set; } = "email"; // email, sms, inapp
    public bool IsActive { get; set; } = true;
}
