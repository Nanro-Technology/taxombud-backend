using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Taxpayers.DTOs;

public record GetTaxpayerComplaintsQuery(
    Guid TaxpayerId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) ;