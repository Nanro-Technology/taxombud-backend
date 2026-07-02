namespace TaxOmbud.Application.Auth.DTOs;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) ;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);