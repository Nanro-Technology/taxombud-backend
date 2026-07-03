using TaxOmbud.Application.Search.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ISearchService
{
    Task<Response<GlobalSearchResultDto>> GlobalSearchAsync(GlobalSearchQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<CaseSearchResultDto>>> SearchCasesAsync(SearchCasesQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<ComplaintSearchResultDto>>> SearchComplaintsAsync(SearchComplaintsQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<DocumentSearchResultDto>>> SearchDocumentsAsync(SearchDocumentsQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<TaxpayerSearchResultDto>>> SearchTaxpayersAsync(SearchTaxpayersQuery request, CancellationToken cancellationToken = default);
}
