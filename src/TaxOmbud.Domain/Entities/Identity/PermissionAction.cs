using System.Runtime.Serialization;

namespace TaxOmbud.Domain.Entities.Identity;

/// <summary>
/// The four CRUD-style actions that can be performed on any module.
/// </summary>
public enum PermissionAction
{
    [EnumMember(Value = "View")]
    View = 1,

    [EnumMember(Value = "Create")]
    Create = 2,

    [EnumMember(Value = "Edit")]
    Edit = 3,

    [EnumMember(Value = "Delete")]
    Delete = 4
}
