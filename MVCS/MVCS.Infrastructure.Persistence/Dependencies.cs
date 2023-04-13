using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCS.Core.Application.Common.Interfaces;
using MVCS.Infrastructure.Persistence.EfCore;

namespace MVCS.Infrastructure.Persistence;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        //var connectionString = configuration.GetConnectionString("ApplicationDbContext");
        //serviceCollection.AddDbContext<IApplicationDBContext, ApplicationDBContext>((s, c) =>
        //{
        //    // var tenantConnectionStringGetter = s.GetService<ITenantConnectionStringGetter>();
        //    c.UseNpgsql(connectionString);
        //});
    }
}