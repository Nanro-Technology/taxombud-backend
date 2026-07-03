using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Search.DTOs;

public record SearchComplaintsQuery(string Term) ;

public record ComplaintSearchResultDto(Guid Id, string ReferenceNumber, string TaxType, string Subject);
