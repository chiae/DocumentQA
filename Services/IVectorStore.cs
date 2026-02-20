
using DocumentQA.Models;

namespace DocumentQA.Services
{
    public interface IVectorStore
    {
        Task StoreAsync(IEnumerable<ChunkEntity> chunks);
        List<ChunkEntity> GetAll();
    }
}
