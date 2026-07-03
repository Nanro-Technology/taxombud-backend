using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record DeleteComplaintCommand(Guid Id) ;
