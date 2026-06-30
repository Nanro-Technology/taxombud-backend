using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Documents.DTOs;

public record GetDocumentByIdQuery(Guid Id) ;

public record DocumentDetailDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    string EntityType,
    Guid EntityId,
    string FilePath,
    DateTimeOffset CreatedAt,
    IEnumerable<DocumentVersionDto> Versions
);

public record DocumentVersionDto(
    Guid Id,
    int VersionNumber,
    string FilePath,
    DateTimeOffset CreatedAt
);