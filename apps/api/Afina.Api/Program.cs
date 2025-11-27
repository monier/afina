using Afina.Data;
using Afina.Api.Endpoints;
using Afina.Core.Interfaces;
using Afina.Modules.Identity.Endpoints.Register;
using Afina.Modules.Identity.Endpoints.Login;
using Afina.Modules.Identity.Services;
using Afina.Modules.Tenant.Endpoints.CreateTenant;
using Afina.Modules.Tenant.Services;
using Afina.Modules.Vault.Endpoints.CreateVaultItem;
using Afina.Modules.Vault.Endpoints.ListVaultItems;
using Afina.Modules.Vault.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AfinaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<AuthService>();
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

// Register migration endpoint (only in Development/Staging)
if (!app.Environment.IsProduction())
{
    app.MapMigrationEndpoint();
}

app.UseHttpsRedirection();
app.UseCors();

// Map endpoints using IEndpoint interface
new RegisterEndpoint().MapEndpoint(app);
new LoginEndpoint().MapEndpoint(app);
new CreateTenantEndpoint().MapEndpoint(app);
new CreateVaultItemEndpoint().MapEndpoint(app);
new ListVaultItemsEndpoint().MapEndpoint(app);

app.Run();
