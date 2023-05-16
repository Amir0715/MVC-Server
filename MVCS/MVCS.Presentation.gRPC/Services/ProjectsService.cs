using Finbuckle.MultiTenant;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCS.Core.Application.Common.Interfaces;
using MVCS.Core.Domain.Entities;
using MVCS.Infrastructure.Identity;
using MVCS.Infrastructure.Persistence;
using MVCS.Presentation.gRPC.AuthorizationHandlers;
using File = MVCS.Core.Domain.Entities.File;

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

    public override async Task<Project> Create(CreateRequest request, ServerCallContext context)
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
        
        return new Project
        {
            Name = project.Name, 
            Identifier = project.Identifier
        };
    }

    [Authorize(Policy = Policies.ProjectPolicy)]
    public override async Task<UploadFilesResponse> UploadFiles(UploadFilesRequest request, ServerCallContext context)
    {
        var branch = await _applicationDbContext.Branches
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Id == request.BranchId, context.CancellationToken);

        if (branch == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Не найдена ветка с id {request.BranchId}"));
        }

        var files = new List<File>();
        foreach (var requestFile in request.Files)
        {
            // Если в ветке существует файл с тем же путем, добавляем новую версию для него
            var file = branch.Files.FirstOrDefault(x => x.Path == requestFile.FilePath);
            file ??= new File(requestFile.FilePath, branch);

            file.AddVersion(requestFile.Content.ToByteArray(), requestFile.Hash);
            branch.AddFile(file);
            files.Add(file);
        }

        await _applicationDbContext.SaveChangesAsync(context.CancellationToken);
        
        var filesResponse = new UploadFilesResponse
        {
            BranchId = branch.Id
        };
        foreach (var file in files)
        {
            var fileResponse = new FileResponse
            {
                Id = file.Id,
                FilePath = file.Path
            };

            foreach (var fileVersion in file.Versions)
            {
                var fileVersionResponse = new FileVersionResponse()
                {
                    Id = fileVersion.Id,
                    Hash = fileVersion.Hash
                };

                fileResponse.Versions.Add(fileVersionResponse);
            }

            filesResponse.Files.Add(fileResponse);
        }

        return filesResponse;
    }

    [Authorize(Policy = Policies.ProjectPolicy)]
    public override async Task<UploadFileResponse> UploadFile(UploadFileRequest request, ServerCallContext context)
    {
        var branch = await _applicationDbContext.Branches
            .Include(x => x.Files).ThenInclude(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == request.BranchId, context.CancellationToken);

        if (branch == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Не найдена ветка с id {request.BranchId}"));
        }

        // Если в ветке существует файл с тем же путем, добавляем новую версию для него
        var file = branch.Files.FirstOrDefault(x => x.Path == request.File.FilePath);
        file ??= new File(request.File.FilePath, branch);

        var fileVersion = file.AddVersion(request.File.Content.ToByteArray(), request.File.Hash);
        branch.AddFile(file);

        await _applicationDbContext.SaveChangesAsync(context.CancellationToken);
        var fileResponse = new UploadFileResponse
        {
            BranchId = branch.Id,
            File = new FileResponse
            {
                Id = file.BranchId,
                FilePath = file.Path,
            }
        };

        fileResponse.File.Versions.Add(file.Versions.Select(x => new FileVersionResponse
        {
            Id = x.Id,
            Hash = x.Hash
        }));
        return fileResponse;
    }

    [Authorize(Policy = Policies.ProjectPolicy)]
    public override async Task<FileResponse> GetFileVersions(GetFileVersionRequest request, ServerCallContext context)
    {
        var branch = await _applicationDbContext.Branches
            .Include(x => x.Files).ThenInclude(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == request.BranchId, context.CancellationToken);

        if (branch == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Не найдена ветка с id {request.BranchId}"));
        }

        File file;
        switch (request.FileOneofCase)
        {
            case GetFileVersionRequest.FileOneofOneofCase.None:
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Необходимо передать file_oneof"));
            case GetFileVersionRequest.FileOneofOneofCase.Id:
                file = branch.Files.FirstOrDefault(x => x.Id == request.Id);
                if (file == null) 
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Файл с id {request.Id} не найден"));
                break;
            case GetFileVersionRequest.FileOneofOneofCase.FilePath:
                file = branch.Files.FirstOrDefault(x => x.Path == request.FilePath);
                if (file == null)
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Файл с path {request.FilePath} не найден"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var fileResponse = new FileResponse
        {
            Id = file.BranchId,
            FilePath = file.Path,
        };

        fileResponse.Versions.Add(file.Versions.Select(x => new FileVersionResponse
        {
            Id = x.Id,
            Hash = x.Hash
        }));
        return fileResponse;
    }

    public override Task<Project> Find(FindRequest request, ServerCallContext context)
    {
        return base.Find(request, context);
    }
}