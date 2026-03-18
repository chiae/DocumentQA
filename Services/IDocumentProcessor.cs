using DocumentQA.Models;

namespace DocumentQA.Services
{
    public interface IDocumentProcessor
    {
        Task<DocumentProcessingResult> ProcessAsync(string documentId, string fileName, string checksum, string userId);
    }
}
