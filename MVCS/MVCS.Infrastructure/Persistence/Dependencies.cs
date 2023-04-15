using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCS.Core.Application.Common.Interfaces;


namespace MVCS.Infrastructure.Persistence;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<IApplicationDbContext, ApplicationDbContext>();
    }
}