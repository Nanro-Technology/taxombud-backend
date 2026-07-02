using TaxOmbud.Application.PayGrades.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IPayGradesService
{
    Task<Response<CreatedPayGradeResponse>> CreatePayGradeAsync(CreatePayGradeCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeletePayGradeAsync(DeletePayGradeCommand request, CancellationToken cancellationToken = default);
    Task<Response<SavedSalaryProfileResponse>> SaveSalaryProfileAsync(SaveSalaryProfileCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdatePayGradeAsync(UpdatePayGradeCommand request, CancellationToken cancellationToken = default);
    Task<Response<PayGradeDetailDto>> GetPayGradeByIdAsync(GetPayGradeByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<PayGradeDto>>> GetPayGradesAsync(GetPayGradesQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<SalaryProfileDto>>> GetSalaryProfilesAsync(GetSalaryProfilesQuery request, CancellationToken cancellationToken = default);
}
