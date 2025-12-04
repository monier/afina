using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Afina.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace Afina.Modules.Users.Tests;

/// <summary>
/// Shared database fixture to reuse a single PostgreSQL container across all tests.
/// This dramatically improves test performance.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private bool _schemaCreated = false;
    private readonly SemaphoreSlim _schemaLock = new SemaphoreSlim(1, 1);

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("afina_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();
        ConnectionString = _postgresContainer.GetConnectionString();
    }

    public async Task EnsureSchemaCreatedAsync(AfinaDbContext db)
    {
        if (_schemaCreated) return;

        await _schemaLock.WaitAsync();
        try
        {
            if (!_schemaCreated)
            {
                // Initialize schemas before running migrations
                // The connection string from testcontainers doesn't include SearchPath,
                // so we default to "afina_test" for tests
                var initializer = new SchemaInitializer();
                await initializer.InitializeAsync(db, "afina_test");

                // Now run migrations
                await db.Database.MigrateAsync();
                _schemaCreated = true;
            }
        }
        finally
        {
            _schemaLock.Release();
        }
    }

    private string ExtractSchemaFromConnection()
    {
        // Extract SearchPath from connection string (e.g., ";SearchPath=afina_test")
        var searchPathParam = ConnectionString.Split(";")
            .FirstOrDefault(p => p.StartsWith("SearchPath=", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(searchPathParam))
        {
            return searchPathParam.Substring("SearchPath=".Length).Split(",")[0].Trim();
        }

        // Default to afina_test for tests
        return "afina_test";
    }

    public async Task DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
        _schemaLock.Dispose();
    }
}

/// <summary>
/// Collection definition to share the database fixture across all test classes.
/// </summary>
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
