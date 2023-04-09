using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MVCS.Infrastructure.Identity;

public class ApplicationIdentityDBContextSeed
{
    public static async Task SeedAsync(ApplicationIdentityDBContext identityDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        //await roleManager.CreateAsync(new IdentityRole("admin"));

        //var defaultUser = new ApplicationUser { UserName = "demouser@microsoft.com", Email = "demouser@microsoft.com" };
        //await userManager.CreateAsync(defaultUser, AuthorizationConstants.DEFAULT_PASSWORD);

        //string adminUserName = "admin@microsoft.com";
        //var adminUser = new ApplicationUser { UserName = adminUserName, Email = adminUserName };
        //await userManager.CreateAsync(adminUser, AuthorizationConstants.DEFAULT_PASSWORD);
        //adminUser = await userManager.FindByNameAsync(adminUserName);
        //if (adminUser != null)
        //{
        //    await userManager.AddToRoleAsync(adminUser, BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        //}
    }
}