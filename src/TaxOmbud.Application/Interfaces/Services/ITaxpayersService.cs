using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Taxpayers.DTOs;
using TaxOmbud.Application.Complaints.DTOs;
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

public interface ITaxpayersService
{
    Task<Response<object?>> DeactivateTaxpayerAsync(DeactivateTaxpayerCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateTaxpayerAsync(UpdateTaxpayerCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> VerifyTaxpayerAsync(VerifyTaxpayerCommand request, CancellationToken cancellationToken = default);
    Task<Response<TaxpayerDetailDto>> GetCurrentTaxpayerAsync(GetCurrentTaxpayerQuery request, CancellationToken cancellationToken = default);
    Task<Response<TaxpayerDetailDto>> GetTaxpayerByIdAsync(GetTaxpayerByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<TaxpayerDetailDto>> GetTaxpayerByTinAsync(GetTaxpayerByTinQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<ComplaintSummaryDto>>> GetTaxpayerComplaintsAsync(GetTaxpayerComplaintsQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<TaxpayerListDto>>> GetTaxpayersAsync(GetTaxpayersQuery request, CancellationToken cancellationToken = default);
    Task<Response<NinVerificationResponseDto>> VerifyNinAsync(VerifyNinQuery request, CancellationToken cancellationToken = default);
}
