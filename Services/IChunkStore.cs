using DocumentQA.Models;

namespace DocumentQA.Services
{
    public interface IChunkStore
    {
        Task SaveChunksAsync(IEnumerable<ChunkEntity> chunks);
    }
}