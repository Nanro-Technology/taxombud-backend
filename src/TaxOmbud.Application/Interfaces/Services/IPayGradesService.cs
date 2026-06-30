using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.PayGrades.DTOs;
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
