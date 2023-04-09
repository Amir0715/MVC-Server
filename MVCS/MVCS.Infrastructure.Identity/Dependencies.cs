using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MVCS.Infrastructure.Identity;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        var connectionString = configuration.GetConnectionString("DbConnection");
        serviceCollection.AddDbContext<ApplicationIdentityDBContext>(c => c.UseNpgsql(connectionString));
    }
}