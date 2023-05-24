using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCS.Core.Application.Common.Interfaces;
using MVCS.Core.Domain.Entities;

namespace MVCS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IMultiTenantDbContext, IApplicationDbContext
{
    private readonly bool _inMemory;
    public ITenantInfo TenantInfo { get; }
    public TenantMismatchMode TenantMismatchMode { get; }
    public TenantNotSetMode TenantNotSetMode { get; }
    public DbSet<Branch> Branches { get; set; }


    public ApplicationDbContext(ITenantInfo tenantInfo)
    {
        TenantInfo = tenantInfo;
    }

    public ApplicationDbContext(ITenantInfo tenantInfo, DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        TenantInfo = tenantInfo;
        Database.Migrate();
    }

    internal ApplicationDbContext(ITenantInfo tenantInfo, DbContextOptions<ApplicationDbContext> options, bool inMemory)
    {
        _inMemory = inMemory;
        TenantInfo = tenantInfo;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureMultiTenant();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_inMemory)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ForCreateMigration;Username=postgres;Password=TESTPASSWORD");
        }
        else
        {
            optionsBuilder.UseNpgsql(TenantInfo?.ConnectionString);
        }
        base.OnConfiguring(optionsBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        this.EnforceMultiTenant();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        this.EnforceMultiTenant();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges()
    {
        this.EnforceMultiTenant();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.EnforceMultiTenant();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
}