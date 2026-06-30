using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Geo.DTOs;

public record GetCountriesQuery() ;

public record CountryDto(string Id, string Name);