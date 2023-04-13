﻿using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using MVCS.Infrastructure.Identity;
using MVCS.Infrastructure.Identity.MultiTenants;

namespace MVCS.Presentation.gRPC.Services;

[Authorize]
public class ProjectsService : Projects.ProjectsBase
{
    private readonly ApplicationIdentityDBContext _dbContext;

    public ProjectsService(ApplicationIdentityDBContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
    {
        var project = new ProjectTenant()
        {
            Name = request.Name
        };
        
        //await _dbContext.(project);
        //await _dbContext.SaveChangesAsync(CancellationToken.None);

        return new CreateResponse()
        {
            Id = 1,
            Name = project.Name
        };
    }
}