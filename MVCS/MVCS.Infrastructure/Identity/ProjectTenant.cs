using Finbuckle.MultiTenant;

namespace MVCS.Infrastructure.Identity;

public class ProjectTenant : ITenantInfo
{
    public string Id { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    private List<ApplicationUser> _users = new();
    public IReadOnlyCollection<ApplicationUser> Users => _users.AsReadOnly();

    public ProjectTenant()
    {

    }

    public ProjectTenant(string name)
    {
        Id = Guid.NewGuid().ToString();
        Identifier = Id;
        Name = name;
    }
}