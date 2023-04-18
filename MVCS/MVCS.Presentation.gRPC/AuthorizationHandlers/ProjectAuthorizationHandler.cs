using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCS.Infrastructure.Identity;

namespace MVCS.Presentation.gRPC.AuthorizationHandlers;

public class ProjectAuthorizationHandler : AuthorizationHandler<ProjectRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IdentityDbContext _identityDbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor, 
        IdentityDbContext identityDbContext, 
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _identityDbContext = identityDbContext ?? throw new ArgumentNullException(nameof(identityDbContext));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectRequirement requirement)
    {
        // Получаем идентификатор проекта из заголовка запроса
        if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("ProjectId", out var tenantIds))
        {
            if (tenantIds.Count == 0)
            {
                context.Fail(new AuthorizationFailureReason(this, "Необходимо передать заголовок ProjectId"));
                return;
            }
        }

        var tenantId = tenantIds.First();
        // Проверяем, имеет ли текущий пользователь доступ к проекту
        if (await UserHasAccessToTenant(context.User, tenantId))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<bool> UserHasAccessToTenant(ClaimsPrincipal userPrincipal, string tenantId)
    {
        var user = await _userManager.GetUserAsync(userPrincipal);
        var projectTenant = await _identityDbContext.ProjectTenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Identifier == tenantId && 
                                      x.Users.Any(u => u.Id == user.Id));
        return projectTenant != null;
    }
}
