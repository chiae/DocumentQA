using System.Security.Cryptography;

namespace DocumentQA.Services
{
    public class FileHashService : IFileHashService
    {
        public string ComputeSha256(Stream stream)
        {
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(stream);
            return Convert.ToHexString(hashBytes);
        }
    }

}
