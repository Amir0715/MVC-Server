using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using MVCS.Infrastructure.Identity;

namespace MVCS.Infrastructure.Persistence;


public class DesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var tenantInfo = new ProjectTenant { ConnectionString = "Data Source=Data/Shared.db" };
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        return new ApplicationDbContext(tenantInfo, optionsBuilder.Options);
    }
}