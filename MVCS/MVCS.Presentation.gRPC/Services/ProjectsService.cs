using Finbuckle.MultiTenant;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCS.Core.Application.Common.Interfaces;
using MVCS.Core.Domain.Entities;
using MVCS.Infrastructure.Identity;
using MVCS.Infrastructure.MultiTenants;

namespace MVCS.Presentation.gRPC.Services;

[Authorize]
public class ProjectsService : Projects.ProjectsBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMultiTenantStore<ProjectTenant> _multiTenantStore;
    private readonly IApplicationDbContext _applicationDbContext;

    public ProjectsService(
        UserManager<ApplicationUser> userManager, 
        IMultiTenantStore<ProjectTenant> multiTenantStore, 
        IApplicationDbContext applicationDbContext)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _multiTenantStore = multiTenantStore ?? throw new ArgumentNullException(nameof(multiTenantStore));
        _applicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
    }

    public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
    {
        var project = new ProjectTenant
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name, 
            ConnectionString = $"Host=mvcs.postgres;Port=5432;Database={request.Name};Username=postgres;Password=MySecretPa$w0rd"
        };
        var successAdded = await _multiTenantStore.TryAddAsync(project);
        if (successAdded)
        {
            var applicationUser = await _userManager.GetUserAsync(context.GetHttpContext().User);
            applicationUser = await _userManager.Users.Include(x => x.Projects)
                .FirstOrDefaultAsync(x => x.Id == applicationUser.Id);
            applicationUser.AddProject(project);
            
            await _userManager.UpdateAsync(applicationUser);
            return new CreateResponse
            {
                Name = project.Name, 
                Identifier = project.Identifier
            };
        }

        return new CreateResponse();
    }

    public override async Task<CreatePostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
    {
        var post = new Post
        {
            Name = request.Name
        };
        await _applicationDbContext.Posts.AddAsync(post);
        await _applicationDbContext.SaveChangesAsync(context.CancellationToken);

        return new CreatePostResponse
        {
            Id = post.Id,
            Name = post.Name
        };
    }
}