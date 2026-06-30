using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;

namespace TaxOmbud.Persistence.Data;

public class MySqlApplicationDbContext : ApplicationDbContext
{
    public MySqlApplicationDbContext(
        DbContextOptions<MySqlApplicationDbContext> options,
        ICurrentUser currentUser)
        : base(options, currentUser)
    {
    }
}
