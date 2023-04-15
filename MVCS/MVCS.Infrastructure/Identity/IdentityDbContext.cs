using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MVCS.Infrastructure.Identity;

public class IdentityDbContext : IdentityDbContext<ApplicationUser>, IMultiTenantStore<ProjectTenant>
{
    private readonly IConfiguration _configuration;
    public DbSet<ProjectTenant> ProjectTenants => Set<ProjectTenant>();

    public IdentityDbContext(IConfiguration configuration, DbContextOptions<IdentityDbContext> options) : base(options)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        builder.Entity<ProjectTenant>().HasKey(ti => ti.Id);
        builder.Entity<ProjectTenant>().Property(ti => ti.Id).HasMaxLength(64);
        builder.Entity<ProjectTenant>().HasIndex(ti => ti.Identifier).IsUnique();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("IdentityDbConnection");
        optionsBuilder.UseNpgsql(connectionString);
        base.OnConfiguring(optionsBuilder);
    }
    
    public async Task<bool> TryAddAsync(ProjectTenant tenantInfo)
    {
        await ProjectTenants.AddAsync(tenantInfo);
        var result = await SaveChangesAsync() > 0;
        Entry(tenantInfo).State = EntityState.Detached;

        return result;
    }

    public async Task<bool> TryUpdateAsync(ProjectTenant tenantInfo)
    {
        ProjectTenants.Update(tenantInfo);
        var result = await SaveChangesAsync() > 0;
        Entry(tenantInfo).State = EntityState.Detached;
        return result;
    }

    public async Task<bool> TryRemoveAsync(string identifier)
    {
        var existing = await ProjectTenants
            .Where(ti => ti.Identifier == identifier)
            .SingleOrDefaultAsync();

        if (existing is null)
        {
            return false;
        }

        ProjectTenants.Remove(existing);
        return await SaveChangesAsync() > 0;
    }

    public async Task<ProjectTenant?> TryGetByIdentifierAsync(string identifier)
    {
        return await ProjectTenants.AsNoTracking()
            .Where(ti => ti.Identifier == identifier)
            .SingleOrDefaultAsync();
    }

    public async Task<ProjectTenant?> TryGetAsync(string id)
    {
        return await ProjectTenants.AsNoTracking()
            .Where(ti => ti.Id == id)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<ProjectTenant>> GetAllAsync()
    {
        return await ProjectTenants.AsNoTracking().ToListAsync();
    }
}