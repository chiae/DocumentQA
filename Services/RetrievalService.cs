using DocumentQA.Data;
using DocumentQA.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentQA.Services
{
    public class RetrievalService : IRetrievalService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly VectorDbContext _dbContext;

        public RetrievalService(IEmbeddingService embeddingService, VectorDbContext dbContext)
        {
            _embeddingService = embeddingService;
            _dbContext = dbContext;
        }

        public async Task<List<ChunkEntity>> RetrieveRelevantChunksAsync(string query, string documentId, int topK = 3)
        {
            // 1. Embed the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

            // 2. Start with base query
            var chunkQuery = _dbContext.Chunks.AsQueryable();

            // 3. Filter by document if provided
            if (!string.IsNullOrEmpty(documentId))
            {
                chunkQuery = chunkQuery.Where(c => c.DocumentId == documentId);
            }
            // 4.Load only the relevant chunks from SQL
            var filteredChunks = await chunkQuery.ToListAsync();



            // 3. Score by cosine similarity
            var scored = filteredChunks
                .Select(c => new
                {
                    Chunk = c,
                    Score = CosineSimilarity(queryEmbedding, c.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x.Chunk)
                .ToList();

            return scored;
        }

        private float CosineSimilarity(float[] a, float[] b)
        {
            float dotProduct = 0;
            float aMag = 0;
            float bMag = 0;

            var len = Math.Min(a.Length, b.Length); // safety if lengths ever differ

            for (int i = 0; i < len; i++)
            {
                dotProduct += a[i] * b[i];
                aMag += a[i] * a[i];
                bMag += b[i] * b[i];
            }

            var denom = (float)(Math.Sqrt(aMag) * Math.Sqrt(bMag) + 1e-8);
            return dotProduct / denom;
        }
    }
}