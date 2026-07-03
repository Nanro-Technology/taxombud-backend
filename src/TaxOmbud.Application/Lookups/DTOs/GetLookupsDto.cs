using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Lookups.DTOs;

public record GetLookupsQuery(string Type);

public record LookupDto(Guid Id, string Name, string Code, string? Description, int? SortOrder);
