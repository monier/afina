using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Afina.Data;
using Xunit;

namespace Afina.Modules.Users.Tests;

[Collection("Database collection")]
public class UsersIntegrationTestBase : IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    protected WebApplicationFactory<Program> Factory { get; private set; } = default!;
    protected HttpClient Client { get; private set; } = default!;

    public UsersIntegrationTestBase(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    public async Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("test");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AfinaDbContext>>();
                    services.AddDbContext<AfinaDbContext>(options =>
                        options.UseNpgsql(_dbFixture.ConnectionString));
                });
            });

        // Create client that doesn't throw on error status codes
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        // Ensure database schema is created once
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AfinaDbContext>();
        await _dbFixture.EnsureSchemaCreatedAsync(db);
    }

    public async Task DisposeAsync()
    {
        // Clean up data after each test to maintain isolation
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AfinaDbContext>();

        // Delete all data but keep schema - include all tables used in tests
        await db.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE ""ApiKeys"" CASCADE;
            TRUNCATE TABLE ""RefreshTokens"" CASCADE;
            TRUNCATE TABLE ""Users"" CASCADE;
        ");

        await Factory.DisposeAsync();
    }
}
