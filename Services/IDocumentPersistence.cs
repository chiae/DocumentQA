using DocumentQA.Models;

namespace DocumentQA.Services
{
    public interface IDocumentPersistence
    {
        Task SaveDocumentAsync(DocumentEntity doc, IEnumerable<ChunkEntity> chunks);
    }
}
