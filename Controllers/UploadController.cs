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
        private readonly IFileStorage _fileStore;
        private readonly IDocumentProcessor _docProcessor;
        private readonly IFileHashService _fileHashService;

        public UploadController(
            IPdfTextExtractor extractor,
            ITextChunker chunker,
            IEmbeddingService embeddingService,
            IChunkStore chunkStore,
            ILlmService llm,
            IOcrService ocr,
            IFileStorage fileStore,
            IDocumentProcessor docProcessor,
            IFileHashService fileHashService,
            VectorDbContext db)
        {
            _extractor = extractor;
            _chunker = chunker;
            _embeddingService = embeddingService;
            _chunkStore = chunkStore;
            _llm = llm;
            _ocr = ocr;
            _fileStore = fileStore;
            _docProcessor = docProcessor;
            _fileHashService = fileHashService;
            _db = db;
        }

        [HttpPost]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {

            using var memory = new MemoryStream();
            await file.CopyToAsync(memory);
            memory.Position = 0;

            // Compute checksum
            var checksum = _fileHashService.ComputeSha256(memory);

            // Check for duplicates
            if (_db.Documents.Any(d => d.Checksum == checksum))
            {
                return BadRequest("This file has already been uploaded.");
            }

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are allowed.");

            if (_db.Documents.Any(d=> d.FileName == file.FileName))

            {
                return BadRequest("File has already been added.");
            }

            var documentId = Guid.NewGuid().ToString("N");

            using (var stream = file.OpenReadStream())
                await _fileStore.SaveAsync(documentId, stream);
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _docProcessor.ProcessAsync(documentId, file.FileName, checksum, userId);

            return Ok(new
            {
                message = "PDF processed and stored successfully",
                documentId
            });
        }

    }
}