using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Exceptions;
using Mapster;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Taxpayers.DTOs;

public record GetTaxpayerByTinQuery(string Tin) ;
