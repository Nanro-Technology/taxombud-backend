using TaxOmbud.Application.Lookups.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ILookupsService
{
    Task<Response<IReadOnlyList<LookupDto>>> GetLookupsAsync(GetLookupsQuery request, CancellationToken cancellationToken = default);
}
