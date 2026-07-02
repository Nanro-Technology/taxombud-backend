using TaxOmbud.Application.Departments.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IDepartmentsService
{
    Task<Response<CreateDepartmentResponse>> CreateDepartmentAsync(CreateDepartmentCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateDepartmentAsync(UpdateDepartmentCommand request, CancellationToken cancellationToken = default);
    Task<Response<DepartmentDto>> GetDepartmentByIdAsync(GetDepartmentByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<DepartmentDto>>> GetDepartmentsAsync(GetDepartmentsQuery request, CancellationToken cancellationToken = default);
}
