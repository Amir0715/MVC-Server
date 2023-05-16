namespace MVCS.Core.Domain.Entities;

/// <summary>
/// Сущность файла в репозитории
/// </summary>
public class File : BaseEntity
{
    public string Path { get; private set; }
    public int BranchId { get; set; }
    public Branch Branch { get; set; }
    public List<FileVersion> Versions { get; private set; }

    protected File() {}

    public File(string path, Branch branch)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        Branch = branch;
        BranchId = branch.Id;

        Path = path;
        Versions ??= new List<FileVersion>();
    }

    public FileVersion AddVersion(byte[] content, string hash)
    {
        var fileVersion = new FileVersion(this, content, hash);
        Versions.Add(fileVersion);
        return fileVersion;
    }
}