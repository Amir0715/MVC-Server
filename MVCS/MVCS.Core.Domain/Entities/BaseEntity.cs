namespace MVCS.Core.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDateTime { get; set; } = DateTime.UtcNow;
}