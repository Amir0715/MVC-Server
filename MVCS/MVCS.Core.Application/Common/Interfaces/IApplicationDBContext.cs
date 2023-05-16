using Microsoft.EntityFrameworkCore;
using MVCS.Core.Domain.Entities;

namespace MVCS.Core.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Branch> Branches { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}