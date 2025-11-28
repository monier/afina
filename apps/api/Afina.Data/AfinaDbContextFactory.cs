using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Afina.Data;

public class AfinaDbContextFactory : IDesignTimeDbContextFactory<AfinaDbContext>
{
    public AfinaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AfinaDbContext>();

        // Use a connection string for design-time operations
        // This is only used for migrations generation, not at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=afina_db;Username=postgres;Password=postgres");

        return new AfinaDbContext(optionsBuilder.Options);
    }
}
