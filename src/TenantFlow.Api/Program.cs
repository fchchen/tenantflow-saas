using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TenantFlow.Api.Middleware;
using TenantFlow.Api.Models;
using TenantFlow.Api.Services;
using TenantFlow.Data;
using TenantFlow.Data.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("Jwt:Key must be configured for non-development environments.");
    }

    // Development fallback so local runs and tests work without committing a real secret.
    jwtSettings.Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

builder.Services.AddSingleton(Options.Create(jwtSettings));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tenantflow.db";
builder.Services.AddDbContext<TenantFlowDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<IMutableTenantContext>(sp => sp.GetRequiredService<TenantContext>());
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IUsageMeteringService, UsageMeteringService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrWhiteSpace(authHeader)
                    && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader["Bearer ".Length..].Trim();
                    return Task.CompletedTask;
                }

                if (context.Request.Cookies.TryGetValue("tenantflow_auth", out var cookieToken)
                    && !string.IsNullOrWhiteSpace(cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdmin", p => p.RequireRole("PlatformAdmin"));
    options.AddPolicy("TenantAdmin", p => p.RequireRole("TenantAdmin", "PlatformAdmin"));
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:4200", "http://127.0.0.1:4200", "http://127.0.0.1:4300"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Ui", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TenantFlowDbContext>();
    await SeedData.EnsureSeededAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Ui");
app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));
app.MapControllers();

app.Run();

public partial class Program;
