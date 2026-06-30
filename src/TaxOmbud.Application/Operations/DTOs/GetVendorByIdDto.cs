using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Domain.Entities.Operations;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record GetVendorByIdQuery(Guid Id) ;