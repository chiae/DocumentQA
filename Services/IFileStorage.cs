namespace DocumentQA.Services
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(string documentId, Stream fileStream);
        Task<Stream> OpenReadAsync(string documentId);
    }
}
