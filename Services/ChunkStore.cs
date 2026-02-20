using DocumentQA.Data;
using DocumentQA.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentQA.Services
{
    public class ChunkStore : IChunkStore
    {
        private readonly VectorDbContext _db;

        public ChunkStore(VectorDbContext db)
        {
            _db = db;
        }

        public async Task SaveChunksAsync(IEnumerable<ChunkEntity> chunks)
        {
            _db.Chunks.AddRange(chunks);
            await _db.SaveChangesAsync();
        }
    }
}