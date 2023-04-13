using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCS.Infrastructure.Identity.MultiTenants;

namespace MVCS.Infrastructure.Identity;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        var connectionString = configuration.GetConnectionString("DbConnection");
        serviceCollection.AddDbContext<ApplicationIdentityDBContext>(c => c.UseNpgsql(connectionString));

        serviceCollection.AddScoped<KeyHasher>();
    }
}