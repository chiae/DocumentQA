using DocumentQA.Data;
using DocumentQA.Models;
using DocumentQA.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace DocumentQA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IPdfTextExtractor _extractor;
        private readonly ITextChunker _chunker;
        private readonly IEmbeddingService _embeddingService;
        private readonly IChunkStore _chunkStore;
        private readonly VectorDbContext _db;

        public UploadController(
            IPdfTextExtractor extractor,
            ITextChunker chunker,
            IEmbeddingService embeddingService,
            IChunkStore chunkStore,
            VectorDbContext db)
        {
            _extractor = extractor;
            _chunker = chunker;
            _embeddingService = embeddingService;
            _chunkStore = chunkStore;
            _db = db;
        }

        [HttpPost]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are allowed.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // 1. Extract text
            var extractedText = _extractor.ExtractText(memoryStream);

            // 2. Chunk text
            var chunks = _chunker.Chunk(extractedText);

            // 3. Create DocumentEntity
            var document = new DocumentEntity
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                UploadedAt = DateTime.UtcNow,
                PageCount = null, // optional
                ChunkCount = 0
            };

            _db.Documents.Add(document);
            await _db.SaveChangesAsync();

             // 4. Embed chunks
            var embeddedChunks = await _embeddingService.EmbedChunksAsync(chunks);

            // 5. Convert to SQL entities with DocumentId
            var entities = embeddedChunks.Select(c => new ChunkEntity
            {
                DocumentId = document.Id,
                Index = c.Index,
                Text = c.Text,
                Embedding = c.Embedding
            });

            // 6. Store chunks
            await _chunkStore.SaveChunksAsync(entities);

            // 7. Update chunk count
            document.ChunkCount = embeddedChunks.Count;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "PDF processed and stored successfully",
                documentId = document.Id,
                fileName = file.FileName,
                chunkCount = embeddedChunks.Count,
                textPreview = extractedText.Substring(0, Math.Min(300, extractedText.Length))
            });
        }
    }
}