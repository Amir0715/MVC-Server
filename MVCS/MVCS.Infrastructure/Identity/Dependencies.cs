using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCS.Infrastructure.Identity.Services;

namespace MVCS.Infrastructure.Identity;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<IdentityDbContext>();
        serviceCollection.AddScoped<KeyHasher>();
    }
}