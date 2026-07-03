using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Operations;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record DeleteVendorCommand(Guid Id) ;
