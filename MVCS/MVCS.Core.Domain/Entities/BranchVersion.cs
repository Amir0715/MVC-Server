namespace MVCS.Core.Domain.Entities;

public class BranchVersion : BaseEntity
{
    public int BranchId { get; set; }
    public Branch Branch { get; set; }

    public string UUID { get; set; }
    public string FileStructure { get; set; }
}