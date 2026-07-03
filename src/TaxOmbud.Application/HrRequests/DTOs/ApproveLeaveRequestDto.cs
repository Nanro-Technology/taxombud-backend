using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.HrRequests.DTOs;

public record ApproveLeaveRequestCommands(Guid LeaveId, bool Approved) ;
