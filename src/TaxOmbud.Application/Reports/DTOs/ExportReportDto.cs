using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Reports.DTOs;

public record ExportReportCommand(string ReportType, string Format, int? Year);

public record ExportReportDto(string DownloadUrl, string ContentType);



public record ExportReportRequest(string ReportType, string Format, int? Year);
