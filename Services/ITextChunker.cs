using DocumentQA.Models;

public interface ITextChunker
{
    List<TextChunk> Chunk(string text);
}
