using Microsoft.EntityFrameworkCore;
using MVCS.Core.Application.Common.Interfaces;

namespace MVCS.Infrastructure.Persistence.EfCore;

public class ApplicationDBContext : DbContext, IApplicationDBContext 
{
}