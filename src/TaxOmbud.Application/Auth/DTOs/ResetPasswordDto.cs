namespace TaxOmbud.Application.Auth.DTOs;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) ;