using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Geo.Queries;

public record GetStatesQuery(string CountryId) : IRequest<Result<IReadOnlyList<StateDto>>>;

public record StateDto(string Id, string Name);

public class GetStatesQueryHandler : IRequestHandler<GetStatesQuery, Result<IReadOnlyList<StateDto>>>
{
    public Task<Result<IReadOnlyList<StateDto>>> Handle(
        GetStatesQuery request, CancellationToken cancellationToken)
    {
        var states = new List<StateDto>();

        if (string.Equals(request.CountryId, "NG", StringComparison.OrdinalIgnoreCase))
        {
            states.AddRange(new[]
            {
                new StateDto("AB", "Abia"),
                new StateDto("FC", "Abuja (FCT)"),
                new StateDto("LA", "Lagos"),
                new StateDto("KN", "Kano"),
                new StateDto("RV", "Rivers")
            });
        }
        else if (string.Equals(request.CountryId, "US", StringComparison.OrdinalIgnoreCase))
        {
            states.AddRange(new[]
            {
                new StateDto("CA", "California"),
                new StateDto("NY", "New York"),
                new StateDto("TX", "Texas")
            });
        }

        return Task.FromResult(Result<IReadOnlyList<StateDto>>.Success(states.AsReadOnly()));
    }
}
