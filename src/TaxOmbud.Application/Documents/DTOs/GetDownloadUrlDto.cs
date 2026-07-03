using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Documents.DTOs;

public record GetDownloadUrlQuery(Guid Id) ;

public record DocumentDownloadUrlDto(string DownloadUrl);
