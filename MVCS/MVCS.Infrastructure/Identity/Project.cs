using MVCS.Core.Domain.Entities;

namespace MVCS.Infrastructure.Identity;

public class Project : BaseEntity
{
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }
    public string ProjectTenantId { get; set; }
}