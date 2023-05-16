namespace MVCS.Core.Domain.Entities;

/// <summary>
/// Ветка в репозитории
/// </summary>
public class Branch : BaseEntity
{
    public static readonly string DefaultName = "main";
    public string Name { get; set; } = DefaultName;
    
    public int? ParentBranchId { get; set; }
    public Branch? ParentBranch { get; set; }

    public List<File> Files { get; set; }

    public List<BranchVersion> BranchVersions { get; set; }

    protected Branch() {}

    public Branch(string name, Branch? parentBranch = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Имя не может быть пустым", nameof(name));

        ParentBranch = parentBranch;
    }

    public void AddFile(File file)
    {
        Files.Add(file);
    }
}