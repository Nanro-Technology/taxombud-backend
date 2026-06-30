namespace TaxOmbud.Application.Interfaces.InfrastructureService;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
