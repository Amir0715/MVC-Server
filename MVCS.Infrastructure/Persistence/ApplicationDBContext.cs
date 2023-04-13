using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using MVCS.Core.Application.Common.Interfaces;
using MVCS.Infrastructure.MultiTenants;

namespace MVCS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IMultiTenantContext<ProjectTenant>, IApplicationDbContext
{
    public ProjectTenant? TenantInfo { get; set; }
    public StrategyInfo? StrategyInfo { get; set; }
    public StoreInfo<ProjectTenant>? StoreInfo { get; set; }
}