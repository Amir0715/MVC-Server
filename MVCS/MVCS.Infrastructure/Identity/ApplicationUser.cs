using Microsoft.AspNetCore.Identity;

namespace MVCS.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? Key { get; set; }

    private List<ProjectTenant> _projects = new();
    public IReadOnlyCollection<ProjectTenant> Projects => _projects.AsReadOnly();

    public ApplicationUser()
    {
    }

    public void AddProject(ProjectTenant projectTenant)
    {
        _projects.Add(projectTenant);
    }
}