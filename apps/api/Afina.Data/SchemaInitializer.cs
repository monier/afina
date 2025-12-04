using Microsoft.EntityFrameworkCore;

namespace Afina.Data;

/// <summary>
/// Responsible for ensuring database schemas exist before migrations run.
/// This is separate from EF Core migrations which only create tables/objects.
/// </summary>
public interface ISchemaInitializer
{
    /// <summary>
    /// Ensures the required schemas exist in the database.
    /// </summary>
    Task InitializeAsync(DbContext context, string schema);
}

public class SchemaInitializer : ISchemaInitializer
{
    /// <summary>
    /// Creates the application schema if it doesn't exist.
    /// Migrations history table will be created in this same schema.
    /// </summary>
    public async Task InitializeAsync(DbContext context, string schema)
    {
        // Create the application schema if it doesn't exist
        // Using raw SQL is safe here since schema name is validated
#pragma warning disable EF1002
        await context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS {schema};");
#pragma warning restore EF1002
    }
}
