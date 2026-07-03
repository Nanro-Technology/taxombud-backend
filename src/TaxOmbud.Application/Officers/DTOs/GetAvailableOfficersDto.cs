using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Officers.DTOs;

public record GetAvailableOfficersQuery(
    Guid? DepartmentId = null,
    string? Specialisation = null,
    int Page = 1,
    int PageSize = 20
) ;
