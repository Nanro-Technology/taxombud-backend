using BCrypt.Net;
using TaxOmbud.Application.Interfaces.InfrastructureService;

namespace TaxOmbud.Infrastructure.Services;

public class PasswordHasher : TaxOmbud.Application.Interfaces.InfrastructureService.IPasswordHasher, TaxOmbud.Application.Common.Interfaces.IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
