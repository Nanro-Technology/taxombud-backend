using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Geo.Queries;

public record GetCountriesQuery() : IRequest<Result<IReadOnlyList<CountryDto>>>;

public record CountryDto(string Id, string Name);

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<IReadOnlyList<CountryDto>>>
{
    public Task<Result<IReadOnlyList<CountryDto>>> Handle(
        GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = new List<CountryDto>
        {
            new CountryDto("NG", "Nigeria"),
            new CountryDto("US", "United States"),
            new CountryDto("GB", "United Kingdom"),
            new CountryDto("CA", "Canada")
        };

        return Task.FromResult(Result<IReadOnlyList<CountryDto>>.Success(countries.AsReadOnly()));
    }
}
