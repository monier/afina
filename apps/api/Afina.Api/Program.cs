using Afina.Data;
using Afina.Api.Endpoints;
using Afina.Modules.Identity.Endpoints;
using Afina.Modules.Identity.Services;
using Afina.Modules.Tenant.Endpoints;
using Afina.Modules.Tenant.Services;
using Afina.Modules.Vault.Endpoints;
using Afina.Modules.Vault.Services;
using Microsoft.EntityFrameworkCore;

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

// Map endpoints
app.MapIdentityEndpoints();
app.MapTenantEndpoints();
app.MapVaultEndpoints();

app.Run();
