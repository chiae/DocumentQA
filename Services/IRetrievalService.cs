using DocumentQA.Models;

namespace DocumentQA.Services
{
    public interface IRetrievalService
    {
        Task<List<ChunkEntity>> RetrieveRelevantChunksAsync(string query, string documentId, string userId, int topK = 3);
    }
}