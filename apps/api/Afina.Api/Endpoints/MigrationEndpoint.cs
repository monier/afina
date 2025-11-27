using Afina.Data;
using Microsoft.EntityFrameworkCore;

namespace Afina.Api.Endpoints;

public static class MigrationEndpoint
{
    /// <summary>
    /// Registers the migration endpoint. Only available in Development and Staging environments.
    /// </summary>
    public static void MapMigrationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/migrate", async (AfinaDbContext dbContext, IWebHostEnvironment env) =>
        {
            // Double-check environment safety (should never reach here in Production)
            if (env.IsProduction())
            {
                return Results.Problem(
                    title: "Migration endpoint not available",
                    detail: "This endpoint is not available in Production environments.",
                    statusCode: 403
                );
            }

            try
            {
                // Get pending migrations
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                var pendingCount = pendingMigrations.Count();

                if (pendingCount == 0)
                {
                    return Results.Ok(new
                    {
                        success = true,
                        message = "Database is already up to date",
                        appliedMigrations = 0,
                        environment = env.EnvironmentName
                    });
                }

                // Apply migrations
                await dbContext.Database.MigrateAsync();

                return Results.Ok(new
                {
                    success = true,
                    message = $"Successfully applied {pendingCount} migration(s)",
                    appliedMigrations = pendingCount,
                    migrations = pendingMigrations.ToArray(),
                    environment = env.EnvironmentName
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Migration failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("ApplyMigrations")
        .WithTags("Database");
    }
}
