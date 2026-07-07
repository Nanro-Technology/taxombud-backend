using TaxOmbud.Application.Complaints.DTOs;
using TaxOmbud.Application.Taxpayers.DTOs;
using TaxOmbud.Common.Responses;

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
