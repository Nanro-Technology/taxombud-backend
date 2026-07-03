using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Payroll.DTOs;

public record ApprovePayrollCommands(Guid RunId) ;
