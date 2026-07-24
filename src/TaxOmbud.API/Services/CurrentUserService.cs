using System.Security.Claims;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Api.Services;

/// <summary>
/// Resolves current user identity from the HTTP context and makes it available
/// throughout the application layer via DI.
/// </summary>
public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
                         ?? User?.FindFirstValue("email");

    public string? FullName => User?.FindFirstValue("name");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

    public string[] Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    public ClaimsPrincipal? Principal => User;

    public string? UserType => User?.FindFirstValue("user_type")
                            ?? User?.FindFirstValue("usertype");

    public string? IpAddress => _httpContextAccessor.HttpContext.GetClientIpAddress();

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
}
