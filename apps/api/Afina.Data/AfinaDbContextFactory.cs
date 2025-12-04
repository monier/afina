using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Afina.Data;

public class AfinaDbContextFactory : IDesignTimeDbContextFactory<AfinaDbContext>
{
    public AfinaDbContext CreateDbContext(string[] args)
    {
        // Determine base path - look for appsettings in the API app directory
        var basePath = Directory.GetCurrentDirectory();
        var apiAppPath = Path.Combine(basePath, "..", "Afina.ApiApp");

        // If running from Afina.Data directory during migrations, look in sibling Afina.ApiApp
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")) && Directory.Exists(apiAppPath))
        {
            basePath = Path.GetFullPath(apiAppPath);
        }

        // Load configuration for migrations
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();

        var configuration = configBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<AfinaDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=afina_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);
        return new AfinaDbContext(optionsBuilder.Options);
    }
}
