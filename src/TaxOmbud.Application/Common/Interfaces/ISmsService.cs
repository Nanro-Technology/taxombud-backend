using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Common.Interfaces;

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
