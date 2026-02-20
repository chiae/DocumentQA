namespace DocumentQA.Services
{
    public interface IRagService
    {
        Task<string> AskAsync(string question, string? documentId);
    }
}