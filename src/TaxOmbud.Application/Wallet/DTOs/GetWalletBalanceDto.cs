using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Wallet.DTOs;

public record GetWalletBalanceQueries(Guid UserId) ;