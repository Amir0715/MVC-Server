using Microsoft.AspNetCore.Identity;
using MVCS.Infrastructure.MultiTenants;

namespace MVCS.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? Key { get; set; }

    private List<Project> _projects;
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    public ApplicationUser()
    {
        _projects = new List<Project>();
    }

    public void AddProject(ProjectTenant projectTenant)
    {
        var project = new Project
        {
            ApplicationUser = this,
            ApplicationUserId = Id,
            ProjectTenantId = projectTenant.Identifier
        };
        _projects.Add(project);
    }
}