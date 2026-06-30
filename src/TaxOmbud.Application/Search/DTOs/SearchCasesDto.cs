using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Search.DTOs;

public record SearchCasesQuery(string Term) ;

public record CaseSearchResultDto(Guid Id, string ReferenceNumber, string Status);