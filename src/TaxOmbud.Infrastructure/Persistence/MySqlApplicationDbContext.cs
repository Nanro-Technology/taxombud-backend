using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;

namespace TaxOmbud.Infrastructure.Persistence;

public class MySqlApplicationDbContext : ApplicationDbContext
{
    public MySqlApplicationDbContext(
        DbContextOptions<MySqlApplicationDbContext> options,
        ICurrentUser currentUser,
        IMediator mediator)
        : base(options, currentUser, mediator)
    {
    }
}
