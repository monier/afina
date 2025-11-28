using Afina.Data;
using Afina.Api.Endpoints;
using Afina.Core.Interfaces;
using Afina.Infrastructure.Mediator;
using Afina.Modules.Users.Features.Login;
using Afina.Modules.Users.Features.Register;
using Afina.Modules.Users.Features.RefreshToken;
using Afina.Modules.Users.Shared.Services;
using Afina.Modules.Tenant.Endpoints.CreateTenant;
using Afina.Modules.Tenant.Services;
using Afina.Modules.Vault.Endpoints.CreateVaultItem;
using Afina.Modules.Vault.Endpoints.ListVaultItems;
using Afina.Modules.Vault.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AfinaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Afina";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AfinaClients";
var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? "local-dev-signing-key-change-me";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Register mediator for handler dispatch and Users services
builder.Services.AddMediator();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<Afina.Modules.Users.Shared.Persistence.IUserRepository, Afina.Modules.Users.Shared.Persistence.UsersRepository>();
builder.Services.AddScoped<Afina.Modules.Users.Shared.Persistence.IUserSessionsRepository, Afina.Modules.Users.Shared.Persistence.UserSessionsRepository>();
builder.Services.AddScoped<Afina.Modules.Users.Shared.Persistence.IApiKeyRepository, Afina.Modules.Users.Shared.Persistence.ApiKeyRepository>();
// Optionally scan encryption handlers via mediator registration (already scans loaded assemblies)
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<VaultService>();

// Add CORS
builder.Services.AddCors(options =>
{
    var corsOrigins = builder.Configuration["CorsOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      ?? new[] { "http://localhost:3000", "http://localhost:5173" };

    Console.WriteLine($"Configured CORS Origins: {string.Join(", ", corsOrigins)}");

    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Afina API v1");
    });
}

// Ensure database is created for development/test environments even if migrations are missing
if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AfinaDbContext>();
    try
    {
        // If migrations exist, prefer Migrate; otherwise EnsureCreated creates schema from the model
        await db.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Database initialization error: {ex.Message}");
        throw;
    }
}

// Register migration endpoint (only in Development/Staging)
if (!app.Environment.IsProduction())
{
    app.MapMigrationEndpoint();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints using IEndpoint interface
new LoginEndpoint().MapEndpoint(app);
new RefreshTokenEndpoint().MapEndpoint(app);
new RegisterEndpoint().MapEndpoint(app);
new Afina.Modules.Users.Features.GetCurrentUser.GetCurrentUserEndpoint().MapEndpoint(app);
new Afina.Modules.Users.Features.DeleteUser.DeleteUserEndpoint().MapEndpoint(app);
new Afina.Modules.Users.Features.ExportUserData.ExportUserDataEndpoint().MapEndpoint(app);
new Afina.Modules.Users.Features.ListApiKeys.ListApiKeysEndpoint().MapEndpoint(app);
new Afina.Modules.Users.Features.CreateApiKey.CreateApiKeyEndpoint().MapEndpoint(app);
new Afina.Modules.Users.Features.DeleteApiKey.DeleteApiKeyEndpoint().MapEndpoint(app);
new CreateTenantEndpoint().MapEndpoint(app);
new CreateVaultItemEndpoint().MapEndpoint(app);
new ListVaultItemsEndpoint().MapEndpoint(app);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
