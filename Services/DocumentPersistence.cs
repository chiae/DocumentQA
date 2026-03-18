using DocumentQA.Data;
using DocumentQA.Models;

namespace DocumentQA.Services
{
    public class DocumentPersistence : IDocumentPersistence
    {
        private readonly VectorDbContext _db;
        private readonly IChunkStore _chunkStore;

        public DocumentPersistence(VectorDbContext db, IChunkStore chunkStore)
        {
            _db = db;
            _chunkStore = chunkStore;
        }

        public async Task SaveDocumentAsync(DocumentEntity doc, IEnumerable<ChunkEntity> chunks)
        {
            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            await _chunkStore.SaveChunksAsync(chunks);

            doc.ChunkCount = chunks.Count();
            await _db.SaveChangesAsync();
        }
    }
}
