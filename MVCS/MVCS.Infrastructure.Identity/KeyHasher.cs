using System.Security.Cryptography;
using System.Text;

namespace MVCS.Infrastructure.Identity;

public class KeyHasher
{
    public string HashKey(string key)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(key); // Преобразование ключа в байты
            byte[] hash = sha256.ComputeHash(bytes); // Вычисление хэша
            return Convert.ToBase64String(hash); // Преобразование хэша в строку Base64
        }
    }
}