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
                    new StateDto("AD", "Adamawa"),
                    new StateDto("AK", "Akwa Ibom"),
                    new StateDto("AN", "Anambra"),
                    new StateDto("BA", "Bauchi"),
                    new StateDto("BY", "Bayelsa"),
                    new StateDto("BE", "Benue"),
                    new StateDto("BO", "Borno"),
                    new StateDto("CR", "Cross River"),
                    new StateDto("DE", "Delta"),
                    new StateDto("EB", "Ebonyi"),
                    new StateDto("ED", "Edo"),
                    new StateDto("EK", "Ekiti"),
                    new StateDto("EN", "Enugu"),
                    new StateDto("FC", "FCT Abuja"),
                    new StateDto("GO", "Gombe"),
                    new StateDto("IM", "Imo"),
                    new StateDto("JI", "Jigawa"),
                    new StateDto("KD", "Kaduna"),
                    new StateDto("KN", "Kano"),
                    new StateDto("KT", "Katsina"),
                    new StateDto("KE", "Kebbi"),
                    new StateDto("KO", "Kogi"),
                    new StateDto("KW", "Kwara"),
                    new StateDto("LA", "Lagos"),
                    new StateDto("NA", "Nasarawa"),
                    new StateDto("NI", "Niger"),
                    new StateDto("OG", "Ogun"),
                    new StateDto("ON", "Ondo"),
                    new StateDto("OS", "Osun"),
                    new StateDto("OY", "Oyo"),
                    new StateDto("PL", "Plateau"),
                    new StateDto("RV", "Rivers"),
                    new StateDto("SO", "Sokoto"),
                    new StateDto("TA", "Taraba"),
                    new StateDto("YO", "Yobe"),
                    new StateDto("ZA", "Zamfara")
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
