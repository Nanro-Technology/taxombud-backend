using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Hr.DTOs;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.IdentityVerification.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IIdentityVerificationService
{
    Task<Response<IdentityVerificationResponse>> VerifyIdentityAsync(VerifyIdentityCommand request, CancellationToken cancellationToken = default);
}
