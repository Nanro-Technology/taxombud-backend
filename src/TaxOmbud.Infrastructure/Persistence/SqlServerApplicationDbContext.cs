using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;

namespace TaxOmbud.Infrastructure.Persistence;

public class SqlServerApplicationDbContext : ApplicationDbContext
{
    public SqlServerApplicationDbContext(
        DbContextOptions<SqlServerApplicationDbContext> options,
        ICurrentUser currentUser,
        IMediator mediator)
        : base(options, currentUser, mediator)
    {
    }
}
