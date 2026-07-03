using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Hr;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Payroll.DTOs;

public record CreateSalaryProfileCommands(Guid StaffId, decimal BaseSalary) ;
