using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Geo.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;

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

            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
            response.Message = "Countries retrieved successfully.";
            response.Data = countries.AsReadOnly();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
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

            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
            response.Message = "States retrieved successfully.";
            response.Data = states.AsReadOnly();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving states.";
            return response;
        }
    }

}