using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Contact.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Enums;
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

namespace TaxOmbud.Application.Services;

public class ContactService : IContactService
{

    public ContactService(
    )
    {
    }

    public async Task<Response<string>> SubmitContactFormAsync(SubmitContactFormCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<string>
        {
            StatusCode = 200,
            Message = "Message received successfully.",
            Data = "Success"
        };
        return await Task.FromResult(response);
    }

}