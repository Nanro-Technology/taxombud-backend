using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Documents.DTOs;

public record GetDocumentVersionsQuery(Guid DocumentId) ;
