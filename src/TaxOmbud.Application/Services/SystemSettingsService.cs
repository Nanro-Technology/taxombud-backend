using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.SystemSettings.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly IGenericRepository<SystemSetting> _settingRepo;
    private readonly ICacheService _cache;

    public SystemSettingsService(
        IGenericRepository<SystemSetting> settingRepo,
        ICacheService cache)
    {
        _settingRepo = settingRepo;
        _cache = cache;
    }

    public async Task<Response<object?>> ToggleE2eeAsync(ToggleE2eeCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var setting = await _settingRepo.FindAsync(s => s.Key == "E2EE_ENABLED");

            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = "E2EE_ENABLED",
                    Value = request.Enable.ToString(),
                    Description = "Globally enables or disables End-to-End Encryption for API payloads."
                };
                await _settingRepo.AddAsync(setting);
            }
            else
            {
                setting.Value = request.Enable.ToString();
                await _settingRepo.UpdateAsync(setting);
            }

            await _settingRepo.SaveAsync();
            await _cache.RemoveAsync("E2EE_ENABLED", cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "E2EE toggled successfully.";
            response.Data = null;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while toggling E2EE.";
            return response;
        }
    }

    public async Task<Response<E2eeStatusDto>> GetE2eeStatusAsync(GetE2eeStatusQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<E2eeStatusDto>();
        try
        {
            var setting = await _settingRepo.FindAsync(s => s.Key == "E2EE_ENABLED");

            bool isEnabled = false;
            if (setting != null && bool.TryParse(setting.Value, out var val))
            {
                isEnabled = val;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "E2EE status retrieved successfully.";
            response.Data = new E2eeStatusDto(isEnabled);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving E2EE status.";
            return response;
        }
    }
}
