using DocumentQA.Models;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<List<ChunkEntity>> EmbedChunksAsync(List<TextChunk> chunks);
}