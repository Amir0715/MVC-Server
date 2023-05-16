using System.Security.Cryptography;
using System.Text;

namespace MVCS.Core.Domain.Entities;

/// <summary>
/// Конкретная версия файла в репозитории и ветке
/// </summary>
public class FileVersion : BaseEntity
{
    public int FileId { get; set; }
    public File File { get; set; }

    public string Hash { get; set; }
    public byte[] Content { get; set; }

    protected FileVersion() {}

    public FileVersion(File file, byte[] content, string hash)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Hash = hash ?? throw new ArgumentNullException(nameof(hash));
    }
}