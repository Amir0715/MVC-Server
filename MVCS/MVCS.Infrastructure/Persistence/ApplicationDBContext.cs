using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCS.Core.Application.Common.Interfaces;
using MVCS.Core.Domain.Entities;
using MVCS.Infrastructure.MultiTenants;

namespace MVCS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IMultiTenantContext<ProjectTenant>, IApplicationDbContext
{
    public ProjectTenant? TenantInfo { get; set; }
    public StrategyInfo? StrategyInfo { get; set; }
    public StoreInfo<ProjectTenant>? StoreInfo { get; set; }

    public DbSet<Post> Posts { get; }

    public ApplicationDbContext(ITenantInfo tenantInfo)
    {
        TenantInfo = tenantInfo as ProjectTenant;
    }

    public ApplicationDbContext(ITenantInfo tenantInfo, DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        TenantInfo = tenantInfo as ProjectTenant;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureMultiTenant();

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(TenantInfo?.ConnectionString);
        base.OnConfiguring(optionsBuilder);
    }
}