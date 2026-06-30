using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Interfaces.InfrastructureService;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendTemplatedAsync(string to, string templateName, object model, CancellationToken cancellationToken = default);
}
