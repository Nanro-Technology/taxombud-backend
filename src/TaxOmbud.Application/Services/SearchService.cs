using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Search.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Application.Services;

public class SearchService : ISearchService
{
    private readonly IApplicationDbContext _context;

    public SearchService(
        IApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<Response<GlobalSearchResultDto>> GlobalSearchAsync(GlobalSearchQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<GlobalSearchResultDto>();
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Message = "Search completed.";
                response.Data = new GlobalSearchResultDto(new List<SearchResultItem>(), new List<SearchResultItem>(), new List<SearchResultItem>());
                return response;
            }

            var searchTerm = $"%{request.Query}%";

            var complaints = await _context.Complaints
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.ReferenceNumber, searchTerm) || EF.Functions.Like(c.Subject, searchTerm))
                .Take(request.Top)
                .Select(c => new SearchResultItem(
                    c.Id,
                    c.ReferenceNumber,
                    c.Subject,
                    "Complaint",
                    $"/api/v1/complaints/{c.Id}"))
                .ToListAsync(cancellationToken);

            var cases = await _context.Cases
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.CaseNumber.Value, searchTerm) || EF.Functions.Like(c.Subject, searchTerm))
                .Take(request.Top)
                .Select(c => new SearchResultItem(
                    c.Id,
                    c.CaseNumber.Value,
                    c.Subject,
                    "Case",
                    $"/api/v1/cases/{c.Id}"))
                .ToListAsync(cancellationToken);

            var taxpayers = await _context.Taxpayers
                .AsNoTracking()
                .Where(t => EF.Functions.Like(t.FirstName, searchTerm)
                         || EF.Functions.Like(t.LastName, searchTerm)
                         || (t.Nin != null && EF.Functions.Like(t.Nin, searchTerm)))
                .Take(request.Top)
                .Select(t => new SearchResultItem(
                    t.Id,
                    $"{t.FirstName} {t.LastName}",
                    t.Email.Value,
                    "Taxpayer",
                    $"/api/v1/taxpayers/{t.Id}"))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Search completed successfully.";
            response.Data = new GlobalSearchResultDto(complaints, cases, taxpayers);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while performing the search.";
            return response;
        }
    }

    public async Task<Response<List<CaseSearchResultDto>>> SearchCasesAsync(SearchCasesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<CaseSearchResultDto>>();
        try
        {
            var term = $"%{request.Term}%";
            var results = await _context.Cases
                .AsNoTracking()
                .Where(c => c.CaseNumber != null && EF.Functions.Like(EF.Property<string>(c, "CaseNumber"), term))
                .Select(c => new CaseSearchResultDto(c.Id, c.CaseNumber != null ? c.CaseNumber.Value : "", c.Status.ToString()))
                .Take(20)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Cases retrieved successfully.";
            response.Data = results;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while searching cases.";
            return response;
        }
    }

    public async Task<Response<List<ComplaintSearchResultDto>>> SearchComplaintsAsync(SearchComplaintsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<ComplaintSearchResultDto>>();
        try
        {
            var term = $"%{request.Term}%";
            var results = await _context.Complaints
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.ReferenceNumber, term) ||
                            EF.Functions.Like(c.TaxType, term))
                .Select(c => new ComplaintSearchResultDto(c.Id, c.ReferenceNumber, c.TaxType, c.Subject))
                .Take(20)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaints retrieved successfully.";
            response.Data = results;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while searching complaints.";
            return response;
        }
    }

    public async Task<Response<List<DocumentSearchResultDto>>> SearchDocumentsAsync(SearchDocumentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<DocumentSearchResultDto>>();
        try
        {
            var term = $"%{request.Term}%";
            var results = await _context.Documents
                .AsNoTracking()
                .Where(d => EF.Functions.Like(d.FileName, term) ||
                            (d.Classification != null && EF.Functions.Like(d.Classification, term)))
                .Select(d => new DocumentSearchResultDto(d.Id, d.FileName, d.Classification ?? ""))
                .Take(20)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Documents retrieved successfully.";
            response.Data = results;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while searching documents.";
            return response;
        }
    }

    public async Task<Response<List<TaxpayerSearchResultDto>>> SearchTaxpayersAsync(SearchTaxpayersQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<TaxpayerSearchResultDto>>();
        try
        {
            var term = $"%{request.Term}%";
            var results = await _context.Taxpayers
                .AsNoTracking()
                .Where(t => (t.TaxId != null && EF.Functions.Like(EF.Property<string>(t, "TaxId"), term)) ||
                            EF.Functions.Like(t.FirstName + " " + t.LastName, term))
                .Select(t => new TaxpayerSearchResultDto(t.Id, t.TaxId != null ? t.TaxId.Value : "", t.FirstName + " " + t.LastName))
                .Take(20)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayers retrieved successfully.";
            response.Data = results;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while searching taxpayers.";
            return response;
        }
    }
}
