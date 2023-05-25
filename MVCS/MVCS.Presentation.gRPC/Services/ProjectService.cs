using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MVCS.Core.Application.Common.Interfaces;
using MVCS.Presentation.gRPC.AuthorizationHandlers;
using MVCS.Presentation.gRPC.Messages;
using File = MVCS.Core.Domain.Entities.File;

namespace MVCS.Presentation.gRPC.Services;

public class ProjectService : Project.ProjectBase
{
    private readonly IApplicationDbContext _applicationDbContext;

    public ProjectService(IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
    }

    [Authorize(Policy = Policies.ProjectPolicy)]
    public override async Task<Branch> CreateBranch(CreateBranchRequest request, ServerCallContext context)
    {
        Core.Domain.Entities.Branch? parentBranch = null;
        if (request.HasParentBranchId)
        {
            parentBranch = await _applicationDbContext.Branches.FindAsync(request.ParentBranchId);
            if (parentBranch == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Не найдена ветка с id {request.ParentBranchId}"));
            }
        }

        var branch = new Core.Domain.Entities.Branch(request.Name, parentBranch);

        await _applicationDbContext.Branches.AddAsync(branch, context.CancellationToken);
        await _applicationDbContext.SaveChangesAsync(context.CancellationToken);

        return new Branch()
        {
            Id = branch.Id,
            Name = branch.Name,
            ParentBranchId = branch.ParentBranchId.GetValueOrDefault()
        };
    }

    [Authorize(Policy = Policies.ProjectPolicy)]
    public override async Task<GetBranchesResponse> GetAllBranches(Empty request, ServerCallContext context)
    {
        var branches = await _applicationDbContext.Branches.AsNoTracking().ToListAsync();

        var branchesResponse = new GetBranchesResponse();
        var branchesForResponse = branches.Select(x =>
        {
            var branch = new Branch
            {
                Id = x.Id,
                Name = x.Name
            };
            if (x.ParentBranchId.HasValue)
                branch.ParentBranchId = x.ParentBranchId.Value;
            return branch;
        });
        branchesResponse.Branches.Add(branchesForResponse);

        return branchesResponse;
    }

    [Authorize(Policy = Policies.ProjectPolicy)]
    public override async Task<UploadFilesResponse> UploadFiles(UploadFilesRequest request, ServerCallContext context)
    {
        var branch = await _applicationDbContext.Branches
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Id == request.BranchId, context.CancellationToken);

        if (branch == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Не найдена ветка с id {request.BranchId}"));
        }

        var files = new List<Core.Domain.Entities.File>();
        foreach (var requestFile in request.Files)
        {
            // Если в ветке существует файл с тем же путем, добавляем новую версию для него
            var file = branch.Files.FirstOrDefault(x => x.Path == requestFile.FilePath);
            file ??= new Core.Domain.Entities.File(requestFile.FilePath, branch);

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
            throw new RpcException(new Status(StatusCode.NotFound, $"Не найдена ветка с id {request.BranchId}"));
        }

        // Если в ветке существует файл с тем же путем, добавляем новую версию для него
        var file = branch.Files.FirstOrDefault(x => x.Path == request.File.FilePath);
        file ??= new Core.Domain.Entities.File(request.File.FilePath, branch);

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
            throw new RpcException(new Status(StatusCode.NotFound, $"Не найдена ветка с id {request.BranchId}"));
        }

        File file;
        switch (request.FileOneofCase)
        {
            case GetFileVersionRequest.FileOneofOneofCase.None:
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Необходимо передать file_oneof"));
            case GetFileVersionRequest.FileOneofOneofCase.Id:
                file = branch.Files.FirstOrDefault(x => x.Id == request.Id);
                if (file == null)
                    throw new RpcException(new Status(StatusCode.NotFound, $"Файл с id {request.Id} не найден"));
                break;
            case GetFileVersionRequest.FileOneofOneofCase.FilePath:
                file = branch.Files.FirstOrDefault(x => x.Path == request.FilePath);
                if (file == null)
                    throw new RpcException(new Status(StatusCode.NotFound, $"Файл с path {request.FilePath} не найден"));
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

    [Authorize(Policy = Policies.ProjectPolicy)]
    public override async Task<FileListResponse> GetBranchFiles(BranchRequest request, ServerCallContext context)
    {
        int branchId;
        string branchName;

        if (request.BranchCase == BranchRequest.BranchOneofCase.BranchId)
        {
            branchId = request.BranchId;
        }
        else if (request.BranchCase == BranchRequest.BranchOneofCase.BranchName)
        {
            branchName = request.BranchName;
        }
        else
        {
            // Неверный запрос, не указан идентификатор или название ветки
            throw new RpcException(new Status(StatusCode.InvalidArgument, "BranchId or BranchName is required."));
        }

        // Получение списка файлов для указанной ветки (используйте branchId или branchName для получения файлов)

        var fileList = new List<FileResponse>();

        // Пример заполнения списка файлов
        fileList.Add(new FileResponse
        {
            Id = 1,
            FilePath = "path/to/file1",
            Versions =
            {
                new FileVersionResponse { Id = 1, Hash = "hash1" },
                new FileVersionResponse { Id = 2, Hash = "hash2" }
            }
        });

        fileList.Add(new FileResponse
        {
            Id = 2,
            FilePath = "path/to/file2",
            Versions =
            {
                new FileVersionResponse { Id = 1, Hash = "hash3" },
                new FileVersionResponse { Id = 2, Hash = "hash4" },
                new FileVersionResponse { Id = 3, Hash = "hash5" }
            }
        });

        return new FileListResponse { Files = { fileList } };
    }
}