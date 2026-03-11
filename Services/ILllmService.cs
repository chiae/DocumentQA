namespace DocumentQA.Services
{
    public interface ILlmService
    {
        Task<string> AskAsync(string question, string context);
        Task<string> SummarizeAsync(string question,string context);
    }
}