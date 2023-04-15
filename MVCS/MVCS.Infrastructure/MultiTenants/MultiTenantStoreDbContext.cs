using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MVCS.Infrastructure.MultiTenants;

public class MultiTenantStoreDbContext : EFCoreStoreDbContext<ProjectTenant>
{
    private readonly IConfiguration _configuration;

    public MultiTenantStoreDbContext(IConfiguration configuration, DbContextOptions<MultiTenantStoreDbContext> options) : base(options)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("MultiTenantConnection");
        optionsBuilder.UseNpgsql(connectionString);
        base.OnConfiguring(optionsBuilder);
    }
}