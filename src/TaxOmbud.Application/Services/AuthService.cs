using Microsoft.EntityFrameworkCore;
using OtpNet;
using TaxOmbud.Application.Auth.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Services;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
    private readonly IGenericRepository<Role> _roleRepo;
    private readonly IGenericRepository<MfaToken> _mfaTokenRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;

    public AuthService(
        IGenericRepository<User> userRepo,
        IGenericRepository<RefreshToken> refreshTokenRepo,
        IGenericRepository<Role> roleRepo,
        IGenericRepository<MfaToken> mfaTokenRepo,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        ITokenService tokenService
    )
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _roleRepo = roleRepo;
        _mfaTokenRepo = mfaTokenRepo;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task<Response<object?>> ChangePasswordAsync(ChangePasswordCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _userRepo.Query()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash ?? string.Empty))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Current password is incorrect.";
                return response;
            }

            var newHash = _passwordHasher.Hash(request.NewPassword);
            user.SetPasswordHash(newHash);

            // Revoke all refresh tokens so all other sessions are terminated
            var tokens = await _refreshTokenRepo.FindAllAsync(t => t.UserId == user.Id);
            await _refreshTokenRepo.RemoveRangeAsync(tokens);

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> DisableMfaAsync(DisableMfaCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _userRepo.Query()
                .Include(u => u.MfaToken)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            // Require password confirmation before disabling MFA
            if (!_passwordHasher.Verify(request.Password, user.PasswordHash ?? string.Empty))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Password confirmation failed.";
                return response;
            }

            if (user.MfaToken is null || !user.MfaToken.IsEnabled)
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Message = "Success";
                return response;
            }

            user.MfaToken.IsEnabled = false;
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> ForgotPasswordAsync(ForgotPasswordCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepo.FindAsync(u => u.Email == email);

            // Always return success to prevent email enumeration
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Message = "Success";
                return response;
            }

            // Generate a secure random reset token
            var token = Convert.ToBase64String(global::System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-").Replace("/", "_").TrimEnd('=');

            user.SetPasswordResetToken(token);
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            // Send email with reset link
            await _emailService.SendAsync(
                to: user.Email ?? string.Empty,
                subject: "Reset your TaxOmbud password",
                htmlBody: $"Use this token to reset your password: {token}\n\nThis link expires in 1 hour.",
                cancellationToken: cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<LoginResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<LoginResponse>();
        try
        {
            var emailNormalized = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepo.Query()
                .Include(u => u.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email == emailNormalized, cancellationToken);

            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash ?? string.Empty))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Invalid email or password.";
                return response;
            }

            if (!user.IsActive || !user.CanSignIn)
            {
                response.StatusCode = StatusCodes.Status403Forbidden;
                response.Message = "This account has been disabled.";
                return response;
            }

            var roles = user.Role is not null
                ? new List<string> { user.Role.Name }
                : new List<string>();

            // Emit permissions as "Module:Action" strings (matched by auth policies)
            var permissions = user.Role?.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => $"{rp.Permission.Module}:{rp.Permission.Action}")
                .Distinct()
                .ToList() ?? new List<string>();

            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email ?? string.Empty, user.UserType, roles, permissions);
            var (refreshToken, refreshExpiry) = _tokenService.GenerateRefreshToken();

            // Persist refresh token
            var rt = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = refreshExpiry,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepo.AddAsync(rt);
            await _refreshTokenRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: refreshExpiry,
                MfaRequired: false,
                UserId: user.Id,
                FullName: user.FullName,
                Roles: roles.AsReadOnly()
            );
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> LogoutAsync(LogoutCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var token = await _refreshTokenRepo.FindAsync(t => t.Token == request.RefreshToken);

            if (token is not null)
            {
                await _refreshTokenRepo.RemoveAsync(token);
                await _refreshTokenRepo.SaveAsync();
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<RefreshTokenResponse>();
        try
        {
            var storedToken = await _refreshTokenRepo.Query()
                .Include(rt => rt.User)
                    .ThenInclude(u => u.Role)
                        .ThenInclude(r => r!.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken);

            if (storedToken is null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Invalid refresh token.";
                return response;
            }

            if (storedToken.IsRevoked)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Refresh token has been revoked.";
                return response;
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Refresh token has expired. Please log in again.";
                return response;
            }

            var user = storedToken.User;

            var roles = user.Role is not null
                ? new List<string> { user.Role.Name }
                : new List<string>();

            var permissions = user.Role?.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => $"{rp.Permission.Module}:{rp.Permission.Action}")
                .Distinct()
                .ToList() ?? new List<string>();

            // Rotate: revoke old, issue new
            storedToken.Revoke();

            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email ?? string.Empty, user.UserType, roles, permissions);
            var (newRefreshToken, expiry) = _tokenService.GenerateRefreshToken();

            var newRt = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = expiry,
                CreatedAt = DateTime.UtcNow,
                ReplacedByToken = null
            };
            storedToken.ReplacedByToken = newRefreshToken;

            await _refreshTokenRepo.UpdateAsync(storedToken);
            await _refreshTokenRepo.AddAsync(newRt);
            await _refreshTokenRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new RefreshTokenResponse(accessToken, newRefreshToken, expiry);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<RegisterResponse>> RegisterAsync(RegisterCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<RegisterResponse>();
        try
        {
            var emailNormalized = request.Email.Trim().ToLowerInvariant();

            var exists = await _userRepo.ExistsAsync(u => u.Email == emailNormalized);

            if (exists)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = $"An account with email '{request.Email}' already exists.";
                return response;
            }

            // Resolve the default Taxpayer role
            var taxpayerRole = await _roleRepo.FindAsync(r => r.Name == "Taxpayer");

            // Create user
            var emailVo = new Email(request.Email);
            var user = User.Create(request.FirstName, request.LastName, emailVo, request.PhoneNumber, UserType.RegisteredTaxpayer);
            user.SetPasswordHash(_passwordHasher.Hash(request.Password));

            if (taxpayerRole != null)
                user.AssignRole(taxpayerRole.Id);

            await _userRepo.AddAsync(user);
            await _userRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new RegisterResponse(user.Id, user.Email ?? string.Empty, user.FullName);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> ResetPasswordAsync(ResetPasswordCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepo.FindAsync(u => u.Email == email);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Invalid or expired reset token.";
                return response;
            }

            if (user.PasswordResetToken != request.Token ||
                user.PasswordResetTokenExpiresAt < DateTimeOffset.UtcNow)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Invalid or expired reset token.";
                return response;
            }

            var newHash = _passwordHasher.Hash(request.NewPassword);
            user.SetPasswordHash(newHash);
            user.ClearPasswordResetToken();

            // Revoke all existing refresh tokens for security
            var tokens = await _refreshTokenRepo.FindAllAsync(t => t.UserId == user.Id);
            await _refreshTokenRepo.RemoveRangeAsync(tokens);

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<SetupMfaResponse>> SetupMfaAsync(SetupMfaCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SetupMfaResponse>();
        try
        {
            var user = await _userRepo.Query()
                .Include(u => u.MfaToken)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            // Generate a new TOTP secret (160-bit = 20 bytes per RFC 6238)
            var secretBytes = KeyGeneration.GenerateRandomKey(20);
            var secretBase32 = Base32Encoding.ToString(secretBytes);

            // Generate 8 backup codes
            var backupCodes = Enumerable.Range(0, 8)
                .Select(_ => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant())
                .ToList();

            // Hash backup codes for storage
            var hashedBackups = string.Join(",", backupCodes.Select(c => _passwordHasher.Hash(c)));

            if (user.MfaToken is null)
            {
                var mfaToken = new MfaToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    SecretKey = secretBase32,
                    IsEnabled = false,
                    BackupCodesHash = hashedBackups
                };
                await _mfaTokenRepo.AddAsync(mfaToken);
            }
            else
            {
                user.MfaToken.SecretKey = secretBase32;
                user.MfaToken.IsEnabled = false;
                user.MfaToken.BackupCodesHash = hashedBackups;
                await _mfaTokenRepo.UpdateAsync(user.MfaToken);
            }

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            var label = Uri.EscapeDataString(user.Email ?? string.Empty);
            var issuer = Uri.EscapeDataString("TaxOmbud");
            var qrUri = $"otpauth://totp/{issuer}:{label}?secret={secretBase32}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new SetupMfaResponse(
                QrCodeUri: qrUri,
                SecretKey: secretBase32,
                BackupCodes: backupCodes.AsReadOnly()
            );
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> VerifyEmailAsync(VerifyEmailCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _userRepo.FindAsync(u => u.EmailVerificationToken == request.Token);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Invalid or expired verification token.";
                return response;
            }

            if (user.EmailVerificationTokenExpiresAt < DateTimeOffset.UtcNow)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Verification token has expired. Please request a new one.";
                return response;
            }

            if (user.EmailVerified)
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Message = "Success";
                return response;
            }

            user.MarkEmailVerified();
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> VerifyMfaAsync(VerifyMfaCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var mfaToken = await _mfaTokenRepo.FindAsync(m => m.UserId == request.UserId);

            if (mfaToken is null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "MFA has not been set up. Please call /mfa/setup first.";
                return response;
            }

            var secretBytes = Base32Encoding.ToBytes(mfaToken.SecretKey);
            var totp = new Totp(secretBytes);

            var isValid = totp.VerifyTotp(
                request.TotpCode,
                out _,
                new VerificationWindow(previous: 1, future: 1));

            if (!isValid)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Invalid TOTP code.";
                return response;
            }

            mfaToken.IsEnabled = true;
            await _mfaTokenRepo.UpdateAsync(mfaToken);
            await _mfaTokenRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }
}
