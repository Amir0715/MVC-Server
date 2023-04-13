using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MVCS.Infrastructure.Identity;

public class ApplicationIdentityDBContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationIdentityDBContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.UseNpgsql(TenantInfo.ConnectionString ?? throw new InvalidOperationException());
        base.OnConfiguring(optionsBuilder);
    }
}