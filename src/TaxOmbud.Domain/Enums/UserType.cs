using System.Runtime.Serialization;

namespace TaxOmbud.Domain.Enums;

public enum UserType
{
    [EnumMember(Value = "guest_user")]
    GuestUser = 1,

    [EnumMember(Value = "registered_taxpayer")]
    RegisteredTaxpayer = 2,

    [EnumMember(Value = "staff_user")]
    StaffUser = 3
}
