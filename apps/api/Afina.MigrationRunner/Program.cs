using Afina.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Set up configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Error: DefaultConnection not found in configuration");
    Environment.Exit(1);
}

// Extract schema from connection string
var schema = ExtractSchemaFromConnectionString(connectionString) ?? "public";
Console.WriteLine($"Initializing schema: {schema}");

// Set the migrations schema before creating DbContext
AfinaDbContext.SetMigrationsSchema(schema);

// Create DbContext
var options = new DbContextOptionsBuilder<AfinaDbContext>()
    .UseNpgsql(connectionString)
    .Options;

using var dbContext = new AfinaDbContext(options);

// Initialize schemas
var schemaInitializer = new SchemaInitializer();
await schemaInitializer.InitializeAsync(dbContext, schema);

Console.WriteLine("Schemas initialized successfully");

// Run migrations
Console.WriteLine("Running migrations...");
try
{
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("✅ Migrations completed successfully");
    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Migration failed: {ex.Message}");
    Console.WriteLine($"Exception details: {ex}");
    Environment.Exit(1);
}

static string? ExtractSchemaFromConnectionString(string connectionString)
{
    // Extract SearchPath from connection string (e.g., ";SearchPath=afina_test")
    var searchPathParam = connectionString.Split(";")
        .FirstOrDefault(p => p.StartsWith("SearchPath=", StringComparison.OrdinalIgnoreCase));

    if (!string.IsNullOrEmpty(searchPathParam))
    {
        return searchPathParam.Substring("SearchPath=".Length).Split(",")[0].Trim();
    }

    return null;
}
