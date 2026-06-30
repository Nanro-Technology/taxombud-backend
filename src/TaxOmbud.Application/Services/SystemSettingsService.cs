using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.SystemSettings.DTOs;
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

public class SystemSettingsService : ISystemSettingsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public SystemSettingsService(
        IApplicationDbContext context,
        ICacheService cache
    )
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Response<object?>> ToggleE2eeAsync(ToggleE2eeCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "E2EE_ENABLED", cancellationToken);

            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = "E2EE_ENABLED",
                    Value = request.Enable.ToString(),
                    Description = "Globally enables or disables End-to-End Encryption for API payloads."
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.Value = request.Enable.ToString();
            }

            await _context.SaveChangesAsync(cancellationToken);
            
            await _cache.RemoveAsync("E2EE_ENABLED", cancellationToken);

            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
            response.Message = "E2EE toggled successfully.";
            response.Data = null;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while toggling E2EE.";
            return response;
        }
    }

    public async Task<Response<E2eeStatusDto>> GetE2eeStatusAsync(GetE2eeStatusQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<E2eeStatusDto>();
        try
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "E2EE_ENABLED", cancellationToken);

            bool isEnabled = false;
            if (setting != null && bool.TryParse(setting.Value, out var val))
            {
                isEnabled = val;
            }

            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
            response.Message = "E2EE status retrieved successfully.";
            response.Data = new E2eeStatusDto(isEnabled);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving E2EE status.";
            return response;
        }
    }
}