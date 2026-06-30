using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Departments.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IDepartmentsService
{
    Task<Response<CreateDepartmentResponse>> CreateDepartmentAsync(CreateDepartmentCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateDepartmentAsync(UpdateDepartmentCommand request, CancellationToken cancellationToken = default);
    Task<Response<DepartmentDto>> GetDepartmentByIdAsync(GetDepartmentByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<DepartmentDto>>> GetDepartmentsAsync(GetDepartmentsQuery request, CancellationToken cancellationToken = default);
}
