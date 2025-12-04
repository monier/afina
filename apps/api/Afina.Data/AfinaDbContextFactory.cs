using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Afina.Data;

public class AfinaDbContextFactory : IDesignTimeDbContextFactory<AfinaDbContext>
{
    public AfinaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AfinaDbContext>();

        // Read connection string from environment variable or use default for local development
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Database=afina_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new AfinaDbContext(optionsBuilder.Options);
    }
}
