using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MVCS.Core.Domain.Interfaces;
using MVCS.Infrastructure.Identity;
using MVCS.Infrastructure.Identity.Jwt;
using MVCS.Infrastructure.Identity.MultiTenants;
using MVCS.Presentation.gRPC.AuthenticationSchemeHandlers;
using MVCS.Presentation.gRPC.OptionsSetup;
using MVCS.Presentation.gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
// Добавляем контекст для идентити
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(configure => { })
    .AddEntityFrameworkStores<ApplicationIdentityDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ITokenClaimsService, IdentityTokenClaimsService>();
builder.Services.AddHttpContextAccessor();

// настриваем аунтефикацию
var userKeyAuthenticationScheme = "UserKey";
builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services
    .AddAuthentication(userKeyAuthenticationScheme) // здесь используем по дефолту нашу схему аунтефикации
    .AddJwtBearer(options =>
    {
        JwtOptions jwtOptions = new JwtOptions();
        builder.Configuration.GetRequiredSection("Jwt").Bind(jwtOptions);
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateLifetime = false,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        };
    })
    .AddScheme<UserKeyAuthenticationSchemeOption, UserKeyAuthenticationSchemeHandler>(userKeyAuthenticationScheme, null);
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(userKeyAuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

MVCS.Infrastructure.Identity.Dependencies.ConfigureServices(builder.Configuration, builder.Services);
MVCS.Infrastructure.Persistence.Dependencies.ConfigureServices(builder.Configuration, builder.Services);

builder.Services.AddMultiTenant<ProjectTenant>()
    .WithHeaderStrategy("Tenant");

var app = builder.Build();
app.UseMultiTenant();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<AuthService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// TODO: Временное решение применение миграций
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDBContext>();
    var pendingMigrations = db.Database.GetPendingMigrations();
    if (pendingMigrations.Any())
    {
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
