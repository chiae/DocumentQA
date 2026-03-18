using DocumentQA.Data;
using DocumentQA.Models;
using DocumentQA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using UglyToad.PdfPig;

namespace DocumentQA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IPdfTextExtractor _extractor;
        private readonly ITextChunker _chunker;
        private readonly IEmbeddingService _embeddingService;
        private readonly IChunkStore _chunkStore;
        private readonly ILlmService _llm;
        private readonly VectorDbContext _db;
        private readonly IOcrService _ocr;

        public UploadController(
            IPdfTextExtractor extractor,
            ITextChunker chunker,
            IEmbeddingService embeddingService,
            IChunkStore chunkStore,
            ILlmService llm,
            IOcrService ocr,
            VectorDbContext db)
        {
            _extractor = extractor;
            _chunker = chunker;
            _embeddingService = embeddingService;
            _chunkStore = chunkStore;
            _llm = llm;
            _ocr = ocr;
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

            // Get page count
            int pageCount;
            using (var pdf = PdfDocument.Open(memoryStream))
            {
                pageCount = pdf.NumberOfPages;
            }

            memoryStream.Position = 0;

            // 1. Extract text (PDF → OCR fallback)
            var extractedText = _extractor.ExtractText(memoryStream);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                memoryStream.Position = 0;
                extractedText = await _ocr.ExtractTextAsync(memoryStream);
            }

            // Blank documents or unreadable ones return a client error response
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return BadRequest("This document contains no readable text. It may be scanned too poorly or blank.");
            }

            // 2. Chunk text
            var chunks = _chunker.Chunk(extractedText);
            var snippet = string.Join("\n\n", chunks.Take(3).Select(c => c.Text));

            var description = await _llm.SummarizeAsync(
                "Summarize this document in 1–2 sentences.",
                snippet
            );

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 3. Create DocumentEntity
            var document = new DocumentEntity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                FileName = file.FileName,
                Description = description,
                UploadedAt = DateTime.UtcNow,
                PageCount = pageCount,
                ChunkCount = 0
            };

            _db.Documents.Add(document);
            await _db.SaveChangesAsync();

            // 4. Embed chunks
            var embeddedChunks = await _embeddingService.EmbedChunksAsync(chunks);

            // 5. Convert to SQL entities
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
                documentId = document.Id
            });
        }
    }
}