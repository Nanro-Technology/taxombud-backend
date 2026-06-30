namespace TaxOmbud.Common.Utilities;

public static class Constants
{
    public static class Messages
    {
        public const string Success = "Operation completed successfully.";
        public const string Created = "Resource created successfully.";
        public const string Updated = "Resource updated successfully.";
        public const string Deleted = "Resource deleted successfully.";
        public const string NotFound = "Resource not found.";
        public const string Unauthorized = "Authentication required.";
        public const string Forbidden = "Access denied.";
        public const string BadRequest = "Invalid request data.";
        public const string ServerError = "An unexpected error occurred. Please try again.";
        public const string DuplicateEntry = "A record with the provided details already exists.";
        public const string InvalidCredentials = "Invalid email or password.";
        public const string AccountLocked = "Your account has been locked. Please contact support.";
        public const string EmailNotVerified = "Please verify your email address before logging in.";
        public const string OtpSent = "OTP sent to your registered email/phone.";
        public const string OtpInvalid = "The OTP provided is invalid or has expired.";
        public const string OtpVerified = "OTP verified successfully.";
        public const string PasswordReset = "Password reset successfully.";
        public const string TokenRefreshed = "Token refreshed successfully.";
        public const string LogoutSuccess = "Logged out successfully.";
    }

    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Officer = "Officer";
        public const string Taxpayer = "Taxpayer";
        public const string Investigator = "Investigator";
        public const string Auditor = "Auditor";
        public const string CaseManager = "CaseManager";
    }

    public static class ClaimTypes
    {
        public const string UserId = "uid";
        public const string Email = "email";
        public const string Role = "role";
        public const string FullName = "name";
        public const string TaxpayerId = "taxpayer_id";
    }

    public static class CacheKeys
    {
        public const string AllUsers = "all_users";
        public const string AllRoles = "all_roles";
        public const string SystemSettings = "system_settings";
        public static string UserById(string id) => $"user_{id}";
        public static string OtpByEmail(string email) => $"otp_{email}";
    }

    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireOfficerRole = "RequireOfficerRole";
        public const string RequireTaxpayerRole = "RequireTaxpayerRole";
    }
}
