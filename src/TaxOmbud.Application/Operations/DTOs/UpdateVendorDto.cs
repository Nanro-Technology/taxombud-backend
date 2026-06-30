using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Domain.Entities.Operations;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record UpdateVendorCommand(
    Guid Id,
    string Name, 
    string Company, 
    string Email, 
    string Phone,
    string? Designation,
    string? Scope,
    string? ScopeTarget,
    string? Notes
) ;