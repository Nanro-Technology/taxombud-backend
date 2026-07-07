using System.Runtime.Serialization;

namespace TaxOmbud.Domain.Enums;

public enum UserType
{
    [EnumMember(Value = "GuestUser")]
    GuestUser = 1,

    [EnumMember(Value = "RegisteredTaxpayer")]
    RegisteredTaxpayer = 2,

    [EnumMember(Value = "StaffUser")]
    StaffUser = 3
}
