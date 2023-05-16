namespace MVCS.Core.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedDateTime { get; set; }
    public DateTime UpdatedDateTime { get; set; }
}