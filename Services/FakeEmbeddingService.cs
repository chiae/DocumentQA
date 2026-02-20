using DocumentQA.Models;


namespace DocumentQA.Services
{
    public class FakeEmbeddingService : IEmbeddingService
    {
        public Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var vector = text
                .Take(32)
                .Select(c => (float)(c % 32) / 31f)
                .ToArray();

            if (vector.Length < 32)
            {
                var padded = new float[32];
                Array.Copy(vector, padded, vector.Length);
                vector = padded;
            }

            return Task.FromResult(vector);
        }

        public async Task<List<ChunkEntity>> EmbedChunksAsync(List<TextChunk> chunks)
        {
            var list = new List<ChunkEntity>();

            foreach (var chunk in chunks)
            {
                var embedding = await GenerateEmbeddingAsync(chunk.Text);

                list.Add(new ChunkEntity
                {
                    DocumentId = chunk.DocumentId,
                    Index = chunk.Index,
                    Text = chunk.Text,
                    Embedding = embedding
                });
            }

            return list;
        }
    }
}