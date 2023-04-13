using Microsoft.AspNetCore.Identity;

namespace MVCS.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? Key { get; set; }
}