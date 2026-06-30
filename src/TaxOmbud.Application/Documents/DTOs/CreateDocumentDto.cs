using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Documents.DTOs;

public record CreateDocumentCommand(
    string FileName,
    string FilePath,
    string ContentType,
    long FileSize,
    string EntityType,
    Guid EntityId
) ;

public record CreatedDocumentResponse(
    Guid Id,
    string FileName,
    string FilePath,
    string ContentType,
    long FileSize
);