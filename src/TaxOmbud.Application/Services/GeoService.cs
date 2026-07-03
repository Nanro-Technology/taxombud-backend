using TaxOmbud.Application.Geo.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Services;

public class GeoService : IGeoService
{

    public GeoService(
    )
    {
    }

    public async Task<Response<IReadOnlyList<CountryDto>>> GetCountriesAsync(GetCountriesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CountryDto>>();
        try
        {
            var countries = new List<CountryDto>
            {
                new CountryDto("NG", "Nigeria"),
                new CountryDto("US", "United States"),
                new CountryDto("GB", "United Kingdom"),
                new CountryDto("CA", "Canada")
            };

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Countries retrieved successfully.";
            response.Data = countries.AsReadOnly();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving countries.";
            return response;
        }
    }

    public async Task<Response<IReadOnlyList<StateDto>>> GetStatesAsync(GetStatesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<StateDto>>();
        try
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

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "States retrieved successfully.";
            response.Data = states.AsReadOnly();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving states.";
            return response;
        }
    }

}
