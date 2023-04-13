using Finbuckle.MultiTenant;
using MVCS.Infrastructure.Identity;

namespace MVCS.Infrastructure.MultiTenants;

public class ProjectTenant : ITenantInfo
{
    public string Id { get; set; }
    public string? Identifier { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }

    public List<ApplicationUser> Users { get; set; }
}