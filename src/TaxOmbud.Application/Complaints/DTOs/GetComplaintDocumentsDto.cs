using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record GetComplaintDocumentsQuery(Guid ComplaintId)
    ;

public record ComplaintDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    DateTimeOffset UploadedAt
);
