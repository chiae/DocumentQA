namespace DocumentQA.Services
{
    public interface IFileHashService
    {
        string ComputeSha256(Stream stream);
    }

}
