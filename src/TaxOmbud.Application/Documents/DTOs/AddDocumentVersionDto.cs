using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Documents.DTOs;

public record AddDocumentVersionCommand(Guid DocumentId, string FilePath) ;

public record AddedVersionResponse(
    Guid Id,
    int VersionNumber,
    string FilePath
);

public record AddDocumentVersionRequest(string FilePath);
