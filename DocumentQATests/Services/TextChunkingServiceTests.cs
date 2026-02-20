using DocumentQA.Services;
public class TextChunkingServiceTests
{
    [Fact]
    public void Chunk_ShouldSplitTextIntoExpectedChunks()
    {
        var chunkSize = 50;
        var overlap = 10;
        var service = new TextChunker(chunkSize, overlap);

        var text = "This is sentence one. This is sentence two. This is sentence three.";

        var chunks = service.Chunk(text);

        Assert.NotEmpty(chunks);
        Assert.True(chunks.Count > 1, "Expected multiple chunks.");

        var lastOfFirst = chunks[0].Text.Substring(chunks[0].Text.Length - overlap);
        var firstOfSecond = chunks[1].Text.Substring(0, overlap);

        Assert.Equal(lastOfFirst, firstOfSecond);
    }

    [Fact]
    public void Chunk_ShouldHandleShortText()
    {
        var service = new TextChunker(100, 20);
        var text = "Short text.";

        var chunks = service.Chunk(text);

        Assert.Single(chunks);
        Assert.Equal("Short text.", chunks[0].Text);
    }

    [Fact]
    public void Chunk_ShouldNormalizeWhitespace()
    {
        var service = new TextChunker(100, 20);
        var text = "Line one.\n\n\nLine two.\n   Line three.";

        var chunks = service.Chunk(text);

        Assert.Single(chunks);
        Assert.DoesNotContain("  ", chunks[0].Text);
        Assert.DoesNotContain("\n\n", chunks[0].Text);
    }
}