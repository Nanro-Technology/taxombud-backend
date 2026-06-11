using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Commands.ExportReport;

public record ExportReportCommand(string ReportType, string Format, int? Year) : IRequest<Result<ExportReportDto>>;

public record ExportReportDto(string DownloadUrl, string ContentType);

public class ExportReportCommandHandler : IRequestHandler<ExportReportCommand, Result<ExportReportDto>>
{
    public Task<Result<ExportReportDto>> Handle(ExportReportCommand request, CancellationToken cancellationToken)
    {
        // In a real application, we would generate a CSV/PDF, save to blob storage, and return a signed URL.
        // For now, we simulate this process.
        
        var ext = request.Format.ToLower() == "pdf" ? "pdf" : "csv";
        var mime = ext == "pdf" ? "application/pdf" : "text/csv";
        var fakeUrl = $"https://storage.taxombud.com/exports/{request.ReportType}_{request.Year}.{ext}";
        
        return Task.FromResult(Result<ExportReportDto>.Success(new ExportReportDto(fakeUrl, mime)));
    }
}
