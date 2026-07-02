using System.Runtime.Serialization;

namespace TaxOmbud.Domain.Entities.Identity;

/// <summary>
/// Defines all application modules that can be targeted by permissions.
/// Each value maps to a database-seeded Permission row.
/// </summary>
public enum Modules
{
    [EnumMember(Value = "Dashboard")]
    Dashboard = 1,

    [EnumMember(Value = "Complaints")]
    Complaints = 2,

    [EnumMember(Value = "Cases")]
    Cases = 3,

    [EnumMember(Value = "Appeals")]
    Appeals = 4,

    [EnumMember(Value = "Appointments")]
    Appointments = 5,

    [EnumMember(Value = "Documents")]
    Documents = 6,

    [EnumMember(Value = "Communications")]
    Communications = 7,

    [EnumMember(Value = "Users")]
    Users = 8,

    [EnumMember(Value = "Roles")]
    Roles = 9,

    [EnumMember(Value = "Reports")]
    Reports = 10,

    [EnumMember(Value = "HR")]
    HR = 11,

    [EnumMember(Value = "Payroll")]
    Payroll = 12,

    [EnumMember(Value = "Finance")]
    Finance = 13,

    [EnumMember(Value = "Operations")]
    Operations = 14,

    [EnumMember(Value = "Audit")]
    Audit = 15,

    [EnumMember(Value = "System")]
    System = 16,

    [EnumMember(Value = "Taxpayers")]
    Taxpayers = 17,

    [EnumMember(Value = "CRM")]
    CRM = 18
}
