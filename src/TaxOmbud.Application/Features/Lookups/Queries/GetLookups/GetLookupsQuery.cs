using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Lookups.Queries.GetLookups;

public record GetLookupsQuery(string Type) : IRequest<Result<IReadOnlyList<LookupDto>>>;

public record LookupDto(string Code, string Name, string? Description);

public class GetLookupsQueryHandler : IRequestHandler<GetLookupsQuery, Result<IReadOnlyList<LookupDto>>>
{
    public Task<Result<IReadOnlyList<LookupDto>>> Handle(
        GetLookupsQuery request, CancellationToken cancellationToken)
    {
        var type = request.Type.ToLowerInvariant();
        var lookups = new List<LookupDto>();

        switch (type)
        {
            case "leavetypes":
                lookups.Add(new LookupDto("ANNUAL", "Annual Leave", null));
                lookups.Add(new LookupDto("SICK", "Sick Leave", null));
                lookups.Add(new LookupDto("MATERNITY", "Maternity Leave", null));
                lookups.Add(new LookupDto("PATERNITY", "Paternity Leave", null));
                break;
            case "complaintcategories":
                lookups.Add(new LookupDto("DELAY", "Service Delay", null));
                lookups.Add(new LookupDto("PROCESS", "Procedural Defect", null));
                lookups.Add(new LookupDto("CONDUCT", "Unprofessional Conduct", null));
                break;
            case "taxtypes":
                lookups.Add(new LookupDto("PIT", "Personal Income Tax", null));
                lookups.Add(new LookupDto("CIT", "Corporate Income Tax", null));
                lookups.Add(new LookupDto("VAT", "Value Added Tax", null));
                lookups.Add(new LookupDto("PAYE", "Pay As You Earn", null));
                break;
            case "documenttypes":
                lookups.Add(new LookupDto("EVIDENCE", "Supporting Evidence", null));
                lookups.Add(new LookupDto("CORRESPONDENCE", "SARS Correspondence", null));
                lookups.Add(new LookupDto("ID", "Identity Document", null));
                break;
            default:
                return Task.FromResult(Result<IReadOnlyList<LookupDto>>.NotFound($"Lookup type '{type}' is not supported."));
        }

        return Task.FromResult(Result<IReadOnlyList<LookupDto>>.Success(lookups.AsReadOnly()));
    }
}
