using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Appeals.DTOs;
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

public interface IAppealsService
{
    Task<Response<FileAppealResponse>> FileAppealAsync(FileAppealCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ReviewAppealAsync(ReviewAppealCommand request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UploadAppealDocumentAsync(UploadAppealDocumentCommand request, CancellationToken cancellationToken = default);
    Task<Response<AppealDetailDto>> GetAppealByIdAsync(GetAppealByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<AppealDocumentDto>>> GetAppealDocumentsAsync(GetAppealDocumentsQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<AppealListDto>>> GetAppealsAsync(GetAppealsQuery request, CancellationToken cancellationToken = default);
}
