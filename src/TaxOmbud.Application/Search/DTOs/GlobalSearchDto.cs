using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Search.DTOs;

public record GlobalSearchQuery(string Query, int Top = 10) ;

public record GlobalSearchResultDto(
    IReadOnlyList<SearchResultItem> Complaints,
    IReadOnlyList<SearchResultItem> Cases,
    IReadOnlyList<SearchResultItem> Taxpayers
);

public record SearchResultItem(Guid Id, string Title, string Description, string EntityType, string Url);