namespace TaxOmbud.Application.Interfaces.InfrastructureService;

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
