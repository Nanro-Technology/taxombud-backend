using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Search.DTOs;

public record SearchDocumentsQuery(string Term) ;

public record DocumentSearchResultDto(Guid Id, string FileName, string Classification);