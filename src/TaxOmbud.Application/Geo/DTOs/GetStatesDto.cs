using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Geo.DTOs;

public record GetStatesQuery(string CountryId) ;

public record StateDto(string Id, string Name);