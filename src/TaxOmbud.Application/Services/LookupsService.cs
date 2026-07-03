using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Lookups.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Services;

public class LookupsService : ILookupsService
{
    public LookupsService()
    {
    }

    public async Task<Response<IReadOnlyList<LookupDto>>> GetLookupsAsync(GetLookupsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<LookupDto>>();
        try
        {
            await Task.CompletedTask;

            var type = request.Type.ToLowerInvariant();
            var lookups = new List<LookupDto>();

            switch (type)
            {
                case "leavetypes":
                    lookups.Add(new LookupDto(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Annual Leave", "ANNUAL", null, 1));
                    lookups.Add(new LookupDto(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Sick Leave", "SICK", null, 2));
                    lookups.Add(new LookupDto(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Maternity Leave", "MATERNITY", null, 3));
                    lookups.Add(new LookupDto(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Paternity Leave", "PATERNITY", null, 4));
                    break;
                case "complaintcategories":
                    lookups.Add(new LookupDto(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Service Delay", "DELAY", null, 1));
                    lookups.Add(new LookupDto(Guid.Parse("66666666-6666-6666-6666-666666666666"), "Procedural Defect", "PROCESS", null, 2));
                    lookups.Add(new LookupDto(Guid.Parse("77777777-7777-7777-7777-777777777777"), "Unprofessional Conduct", "CONDUCT", null, 3));
                    break;
                case "taxtypes":
                    lookups.Add(new LookupDto(Guid.Parse("88888888-8888-8888-8888-888888888888"), "Personal Income Tax", "PIT", null, 1));
                    lookups.Add(new LookupDto(Guid.Parse("99999999-9999-9999-9999-999999999999"), "Corporate Income Tax", "CIT", null, 2));
                    lookups.Add(new LookupDto(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Value Added Tax", "VAT", null, 3));
                    lookups.Add(new LookupDto(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Pay As You Earn", "PAYE", null, 4));
                    break;
                case "documenttypes":
                    lookups.Add(new LookupDto(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Supporting Evidence", "EVIDENCE", null, 1));
                    lookups.Add(new LookupDto(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "SARS Correspondence", "CORRESPONDENCE", null, 2));
                    lookups.Add(new LookupDto(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Identity Document", "ID", null, 3));
                    break;
                default:
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = $"Lookup type '{type}' is not supported.";
                    return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Lookups retrieved successfully.";
            response.Data = lookups.AsReadOnly();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving lookups.";
            return response;
        }
    }
}
