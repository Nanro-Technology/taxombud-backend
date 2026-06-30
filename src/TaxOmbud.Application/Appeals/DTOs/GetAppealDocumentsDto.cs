using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Appeals.DTOs;

public record GetAppealDocumentsQuery(Guid AppealId) ;

public record AppealDocumentDto(Guid Id, string FileName, string ContentType, long FileSize, DateTimeOffset CreatedAt);