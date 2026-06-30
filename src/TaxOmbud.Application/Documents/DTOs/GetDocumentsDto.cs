using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Documents.DTOs;

public record GetDocumentsQuery(
    Guid? EntityId,
    string? EntityType,
    int Page = 1,
    int PageSize = 20
) ;

public record DocumentListDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    string EntityType,
    Guid EntityId,
    string FilePath,
    DateTimeOffset CreatedAt
);