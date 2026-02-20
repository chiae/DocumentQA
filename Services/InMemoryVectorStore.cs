using DocumentQA.Models;
using System.Collections.Concurrent;

namespace DocumentQA.Services
{
    public class InMemoryVectorStore : IVectorStore
    {
        private readonly ConcurrentBag<ChunkEntity> _chunks = new();

        public Task StoreAsync(IEnumerable<ChunkEntity> chunks)
        {
            foreach (var chunk in chunks)
                _chunks.Add(chunk);
            Console.WriteLine($"Stored {chunks.Count()} chunks. Total now: {_chunks.Count}");

            return Task.CompletedTask;
        }

        public List<ChunkEntity> GetAll()
        {
            return _chunks.ToList();
        }
    }
}