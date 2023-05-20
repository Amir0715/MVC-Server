using Finbuckle.MultiTenant;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MVCS.Core.Domain.Entities;
using MVCS.Infrastructure.Identity;
using MVCS.Infrastructure.Persistence;
using MVCS.Presentation.gRPC.Messages;

namespace MVCS.Presentation.gRPC.Services;

public class ProjectsService : Projects.ProjectsBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMultiTenantStore<ProjectTenant> _multiTenantStore;

    public ProjectsService(
        UserManager<ApplicationUser> userManager, 
        IMultiTenantStore<ProjectTenant> multiTenantStore)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _multiTenantStore = multiTenantStore ?? throw new ArgumentNullException(nameof(multiTenantStore));
    }

    public override async Task<Messages.Project> Create(CreateRequest request, ServerCallContext context)
    {
        var applicationUser = await _userManager.GetUserAsync(context.GetHttpContext().User);
        var project = new ProjectTenant(request.Name)
        {
            ConnectionString = $"Host=mvcs.postgres;Port=5432;Database={request.Name};Username=postgres;Password=MySecretPa$w0rd"
        };
        applicationUser.AddProject(project);
        var successAdded = await _userManager.UpdateAsync(applicationUser);

        if (!successAdded.Succeeded)
            throw new RpcException(new Status(StatusCode.InvalidArgument, successAdded.ToString()));


        var applicationDbContext = new ApplicationDbContext(project);
        await applicationDbContext.Branches.AddAsync(new Branch(Branch.DefaultName), context.CancellationToken);
        await applicationDbContext.SaveChangesAsync(context.CancellationToken);

        return new Messages.Project
        {
            Name = project.Name,
            Identifier = project.Identifier
        };
    }

    [Authorize]
    public override async Task<Messages.Project> Find(FindRequest request, ServerCallContext context)
    {
        var projectName = request.Name;
        var projects = await _multiTenantStore.GetAllAsync();
        var project = projects.FirstOrDefault(x => x.Name == projectName);

        if (project == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Проект с названием {request.Name}"));

        return new Messages.Project()
        {
            Name = project.Name,
            Identifier = project.Identifier
        };
    }

    [Authorize]
    public override async Task<ProjectList> GetAll(Empty request, ServerCallContext context)
    {
        var projects = await _multiTenantStore.GetAllAsync();

        var projectList = new ProjectList();
        //foreach (var projectTenant in projects)
        //{
        //    projectList.Projects.Add(new Project()
        //    {
        //        Name = projectTenant.Name,
        //        Identifier = projectTenant.Identifier
        //    });
        //}

        projectList.Projects.AddRange(projects.Select(x => new Messages.Project()
        {
            Name = x.Name,
            Identifier = x.Identifier
        }));

        return projectList;
    }
}