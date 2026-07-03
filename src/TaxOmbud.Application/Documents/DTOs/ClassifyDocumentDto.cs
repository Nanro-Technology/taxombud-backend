using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Documents.DTOs;

public record ClassifyDocumentCommand(Guid DocumentId, string Classification) ;

public record ClassifyDocumentRequest(string Classification);
