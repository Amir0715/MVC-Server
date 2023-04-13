namespace MVCS.Core.Application.Common.Interfaces;

public interface IApplicationDBContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}