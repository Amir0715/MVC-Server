using Finbuckle.MultiTenant;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MVCS.Core.Application.Common.Interfaces;
using MVCS.Core.Domain.Entities;
using MVCS.Infrastructure.Identity;
using MVCS.Presentation.gRPC.AuthorizationHandlers;

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
        var applicationUser = await _userManager.GetUserAsync(context.GetHttpContext().User);
        var project = new ProjectTenant(request.Name)
        {
            ConnectionString = $"Host=mvcs.postgres;Port=5432;Database={request.Name};Username=postgres;Password=MySecretPa$w0rd"
        };
        applicationUser.AddProject(project);
        var successAdded = await _userManager.UpdateAsync(applicationUser);
        if (successAdded.Succeeded)
        {
            return new CreateResponse
            {
                Name = project.Name, 
                Identifier = project.Identifier
            };
        }

        throw new RpcException(new Status(StatusCode.InvalidArgument, successAdded.ToString()));
    }

    [Authorize(Policy = Policies.ProjectPolicy)]
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