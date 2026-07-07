using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;

namespace TaxOmbud.Persistence.Data;

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
