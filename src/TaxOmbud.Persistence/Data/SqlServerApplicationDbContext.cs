using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;

namespace TaxOmbud.Persistence.Data;

public class SqlServerApplicationDbContext : ApplicationDbContext
{
    public SqlServerApplicationDbContext(
        DbContextOptions<SqlServerApplicationDbContext> options,
        ICurrentUser currentUser)
        : base(options, currentUser)
    {
    }
}
