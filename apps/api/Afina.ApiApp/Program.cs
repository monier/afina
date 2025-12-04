using Afina.ApiApp.Infrastructure.Logging;
using Afina.Core.Configuration;
using Afina.Core.Interfaces;
using Afina.Data;
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
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Afina API";
    config.Version = "v1";
});
builder.Services.AddDbContext<AfinaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
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
builder.Services.AddMediator();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<Afina.Modules.Users.Shared.Persistence.IUserRepository, Afina.Modules.Users.Shared.Persistence.UsersRepository>();
builder.Services.AddScoped<Afina.Modules.Users.Shared.Persistence.IUserSessionsRepository, Afina.Modules.Users.Shared.Persistence.UserSessionsRepository>();
builder.Services.AddScoped<Afina.Modules.Users.Shared.Persistence.IApiKeyRepository, Afina.Modules.Users.Shared.Persistence.ApiKeyRepository>();
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<VaultService>();
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

if (app.Environment.IsDevEnvironment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
    app.MapGet("/swagger/index.html", ctx =>
    {
        ctx.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
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
try
{
    Log.Information("Starting Afina API application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
