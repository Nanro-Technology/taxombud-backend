using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using TaxOmbud.Application.Auth.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
    private readonly IGenericRepository<Role> _roleRepo;
    private readonly IGenericRepository<MfaToken> _mfaTokenRepo;
    private readonly IGenericRepository<TaxpayerProfile> _taxpayerProfileRepo;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IGenericRepository<RefreshToken> refreshTokenRepo,
        IGenericRepository<Role> roleRepo,
        IGenericRepository<MfaToken> mfaTokenRepo,
        IGenericRepository<TaxpayerProfile> taxpayerProfileRepo,
        IEmailService emailService,
        ITokenService tokenService
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _refreshTokenRepo = refreshTokenRepo;
        _roleRepo = roleRepo;
        _mfaTokenRepo = mfaTokenRepo;
        _taxpayerProfileRepo = taxpayerProfileRepo;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    // ─── Taxpayer Self-Registration (public portal) ───────────────────────────

    public async Task<Response<RegisterResponse>> RegisterTaxpayerAsync(RegisterTaxpayerCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<RegisterResponse>();
        try
        {
            if (!request.ConsentGiven)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthConsentRequired;
                return response;
            }

            var emailNormalized = request.Email.Trim().ToLowerInvariant();

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(emailNormalized);
            if (existingUser is not null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = $"An account with email '{request.Email}' already exists.";
                return response;
            }

            // Resolve the Taxpayer role
            // NOTE: Taxpayers do NOT get a role — their UserType.RegisteredTaxpayer IS their identity.
            // Roles are exclusively for StaffUser accounts.

            // Build the user
            var emailVo = new Email(emailNormalized);
            var user = User.Create(
                request.FirstName,
                request.LastName,
                emailVo,
                request.PhoneNumber,
                UserType.RegisteredTaxpayer
            );

            // Create user via UserManager — hashing, validation and lockout seeding are handled internally
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = errors;
                return response;
            }

            // Create the TaxpayerProfile with the additional fields from the signup form
            var profile = TaxpayerProfile.Create(user.Id, TaxpayerType.Individual.ToString());
            profile.Gender = request.Gender;
            profile.Nin = request.Nin;
            profile.Address = request.Address;
            profile.City = request.City;
            profile.State = request.State;
            profile.Country = request.Country;

            await _taxpayerProfileRepo.AddAsync(profile);
            await _taxpayerProfileRepo.SaveAsync();

            // Generate email verification token via Identity and send welcome email
            var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            user.SetEmailVerificationToken(verificationToken);
            await _userManager.UpdateAsync(user);

            // Verification email disabled for local development
            /*
            await _emailService.SendAsync(
                to: user.Email ?? string.Empty,
                subject: "Verify your TaxOmbud account",
                htmlBody: $"<p>Hello {user.FirstName},</p><p>Please verify your email using this token: <strong>{verificationToken}</strong></p><p>This token expires in 24 hours.</p>",
                cancellationToken: cancellationToken);
            */

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.AuthTaxpayerRegistered;
            response.Data = new RegisterResponse(user.Id, user.Email ?? string.Empty, user.FullName, UserType.RegisteredTaxpayer.ToString());
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Staff Registration (admin-only) ─────────────────────────────────────

    public async Task<Response<RegisterResponse>> RegisterAsync(RegisterCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<RegisterResponse>();
        try
        {
            var emailNormalized = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _userManager.FindByEmailAsync(emailNormalized);
            if (existingUser is not null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = $"An account with email '{request.Email}' already exists.";
                return response;
            }

            // Guard: only StaffUser accounts are created via this endpoint.
            // Taxpayers self-register via RegisterTaxpayerAsync. Guests need no account.
            if (request.UserType != UserType.StaffUser)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Only staff accounts can be registered through this endpoint. Taxpayers must use the self-registration portal.";
                return response;
            }

            // Resolve the role to assign — admin specifies RoleId, or we default to Officer
            Role? assignedRole = null;
            if (request.RoleId.HasValue)
            {
                assignedRole = await _roleRepo.GetByIdAsync(request.RoleId.Value);
                if (assignedRole is null)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "The specified role does not exist.";
                    return response;
                }
            }
            else
            {
                // Fallback default
                assignedRole = await _roleRepo.FindAsync(r => r.Name == RoleConstants.Officer);
            }

            var emailVo = new Email(emailNormalized);
            var user = User.Create(
                request.FirstName,
                request.LastName,
                emailVo,
                request.PhoneNumber,
                UserType.StaffUser
            );

            if (assignedRole is not null)
                user.AssignRole(assignedRole.Id);

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = errors;
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Success;
            response.Data = new RegisterResponse(user.Id, user.Email ?? string.Empty, user.FullName, user.UserType.ToString());
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Login ────────────────────────────────────────────────────────────────

    public async Task<Response<LoginResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<LoginResponse>();
        try
        {
            var emailNormalized = request.Email.Trim().ToLowerInvariant();

            var user = await _userManager.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email == emailNormalized, cancellationToken);

            // Generic invalid-credential message to prevent email enumeration
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.InvalidCredentials;
                return response;
            }

            // UserType guard — prevents a taxpayer from logging into the staff portal and vice-versa
            if (user.UserType != request.UserType)
            {
                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = Constants.Messages.InvalidCredentials;
                return response;
            }

            if (!user.IsActive || !user.CanSignIn)
            {
                response.StatusCode = StatusCodes.Status403Forbidden;
                response.Message = Constants.Messages.AuthAccountDisabled;
                return response;
            }

            // Check password via SignInManager (handles lockout, two-factor, etc.)
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!signInResult.Succeeded)
            {
                if (signInResult.IsLockedOut)
                {
                    response.StatusCode = StatusCodes.Status423Locked;
                    response.Message = "Account is locked due to multiple failed attempts. Please try again in 15 minutes.";
                    return response;
                }

                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.InvalidCredentials;
                return response;
            }

            var roles = user.Role is not null
                ? new List<string> { user.Role.Name }
                : new List<string>();

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
            response.Message = Constants.Messages.Success;
            response.Data = new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: refreshExpiry,
                MfaRequired: false,
                UserId: user.Id,
                FullName: user.FullName,
                UserType: user.UserType.ToString(),
                Email: user.Email,
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

    // ─── Logout ───────────────────────────────────────────────────────────────

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
            response.Message = Constants.Messages.LogoutSuccess;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Refresh Token ────────────────────────────────────────────────────────

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
                response.Message = Constants.Messages.AuthInvalidRefreshToken;
                return response;
            }

            if (storedToken.IsRevoked)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthRefreshTokenRevoked;
                return response;
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthRefreshTokenExpired;
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
            response.Message = Constants.Messages.TokenRefreshed;
            response.Data = new RefreshTokenResponse(accessToken, newRefreshToken, expiry);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Change Password ──────────────────────────────────────────────────────

    public async Task<Response<object?>> ChangePasswordAsync(ChangePasswordCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.AuthUserNotFound;
                return response;
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = string.IsNullOrEmpty(errors) ? Constants.Messages.AuthPasswordIncorrect : errors;
                return response;
            }

            // Revoke all refresh tokens so all other sessions are terminated
            var tokens = await _refreshTokenRepo.FindAllAsync(t => t.UserId == user.Id);
            await _refreshTokenRepo.RemoveRangeAsync(tokens);
            await _refreshTokenRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Success;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Forgot Password ──────────────────────────────────────────────────────

    public async Task<Response<object?>> ForgotPasswordAsync(ForgotPasswordCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            // Always return success to prevent email enumeration
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.Messages.Success;
                return response;
            }

            // Generate a secure password reset token via Identity
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Store token on user for lookup during reset
            user.SetPasswordResetToken(token);
            await _userManager.UpdateAsync(user);

            // Password reset email disabled for local development
            /*
            await _emailService.SendAsync(
                to: user.Email ?? string.Empty,
                subject: "Reset your TaxOmbud password",
                htmlBody: $"<p>Use this token to reset your password: <strong>{token}</strong></p><p>This link expires in 1 hour.</p>",
                cancellationToken: cancellationToken);
            */

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Success;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Reset Password ───────────────────────────────────────────────────────

    public async Task<Response<object?>> ResetPasswordAsync(ResetPasswordCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthInvalidResetToken;
                return response;
            }

            // Reset password via Identity (validates the token internally)
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthInvalidResetToken;
                return response;
            }

            user.ClearPasswordResetToken();
            await _userManager.UpdateAsync(user);

            // Revoke all existing refresh tokens for security
            var tokens = await _refreshTokenRepo.FindAllAsync(t => t.UserId == user.Id);
            await _refreshTokenRepo.RemoveRangeAsync(tokens);
            await _refreshTokenRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.PasswordReset;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Verify Email ─────────────────────────────────────────────────────────

    public async Task<Response<object?>> VerifyEmailAsync(VerifyEmailCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token, cancellationToken);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthInvalidVerificationToken;
                return response;
            }

            if (user.EmailVerificationTokenExpiresAt < DateTimeOffset.UtcNow)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthVerificationTokenExpired;
                return response;
            }

            if (user.EmailVerified)
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.Messages.Success;
                return response;
            }

            // Confirm email via Identity
            var confirmResult = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!confirmResult.Succeeded)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthInvalidVerificationToken;
                return response;
            }

            user.MarkEmailVerified();
            await _userManager.UpdateAsync(user);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Success;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── MFA Setup ────────────────────────────────────────────────────────────

    public async Task<Response<SetupMfaResponse>> SetupMfaAsync(SetupMfaCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SetupMfaResponse>();
        try
        {
            var user = await _userManager.Users
                .Include(u => u.MfaToken)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.AuthUserNotFound;
                return response;
            }

            // Generate a new TOTP secret (160-bit = 20 bytes per RFC 6238)
            var secretBytes = KeyGeneration.GenerateRandomKey(20);
            var secretBase32 = Base32Encoding.ToString(secretBytes);

            // Generate 8 backup codes
            var backupCodes = Enumerable.Range(0, 8)
                .Select(_ => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant())
                .ToList();

            // Hash backup codes via Identity's built-in password hasher
            var identityHasher = new PasswordHasher<User>();
            var hashedBackups = string.Join(",", backupCodes.Select(c => identityHasher.HashPassword(user, c)));

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

            await _mfaTokenRepo.SaveAsync();

            var label = Uri.EscapeDataString(user.Email ?? string.Empty);
            var issuer = Uri.EscapeDataString("TaxOmbud");
            var qrUri = $"otpauth://totp/{issuer}:{label}?secret={secretBase32}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Success;
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

    // ─── Verify MFA ───────────────────────────────────────────────────────────

    public async Task<Response<object?>> VerifyMfaAsync(VerifyMfaCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var mfaToken = await _mfaTokenRepo.FindAsync(m => m.UserId == request.UserId);

            if (mfaToken is null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthMfaNotSetUp;
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
                response.Message = Constants.Messages.AuthInvalidTotp;
                return response;
            }

            mfaToken.IsEnabled = true;
            await _mfaTokenRepo.UpdateAsync(mfaToken);
            await _mfaTokenRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.OtpVerified;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    // ─── Disable MFA ──────────────────────────────────────────────────────────

    public async Task<Response<object?>> DisableMfaAsync(DisableMfaCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _userManager.Users
                .Include(u => u.MfaToken)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.AuthUserNotFound;
                return response;
            }

            // Verify password before disabling MFA using UserManager
            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordCheck)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AuthPasswordConfirmationFailed;
                return response;
            }

            if (user.MfaToken is null || !user.MfaToken.IsEnabled)
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.Messages.Success;
                return response;
            }

            user.MfaToken.IsEnabled = false;
            await _mfaTokenRepo.UpdateAsync(user.MfaToken);
            await _mfaTokenRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Success;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }
}
