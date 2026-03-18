using DocumentQA.Models;
using DocumentQA.Services;
using UglyToad.PdfPig;

namespace DocumentQA.Services
{
    public class DocumentProcessor : IDocumentProcessor
    {
        private readonly IFileStorage _storage;
        private readonly IPdfTextExtractor _extractor;
        private readonly IOcrService _ocr;
        private readonly ITextChunker _chunker;
        private readonly ILlmService _llm;
        private readonly IEmbeddingService _embedding;
        private readonly IDocumentPersistence _persistence;

        public DocumentProcessor(
            IFileStorage storage,
            IPdfTextExtractor extractor,
            IOcrService ocr,
            ITextChunker chunker,
            ILlmService llm,
            IEmbeddingService embedding,
            IDocumentPersistence persistence)
        {
            _storage = storage;
            _extractor = extractor;
            _ocr = ocr;
            _chunker = chunker;
            _llm = llm;
            _embedding = embedding;
            _persistence = persistence;
        }

        public async Task<DocumentProcessingResult> ProcessAsync(string documentId, string fileName, string checksum,string userId)
        {
            // 1. Page count
            int pageCount;
            using (var stream = await _storage.OpenReadAsync(documentId))
            using (var pdf = PdfDocument.Open(stream))
                pageCount = pdf.NumberOfPages;

            // 2. Extract text (with OCR fallback)
            string text;
            using (var stream = await _storage.OpenReadAsync(documentId))
            {
                text = _extractor.ExtractText(stream);

                if (string.IsNullOrWhiteSpace(text))
                {
                    stream.Position = 0;
                    text = await _ocr.ExtractTextAsync(stream);
                }
            }

            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Unreadable document");

            // 3. Chunk the text
            var chunks = _chunker.Chunk(text);

            // ⭐ NEW: Filter out empty/whitespace chunks BEFORE embedding
            var cleanChunks = chunks
                .Where(c => !string.IsNullOrWhiteSpace(c.Text))
                .ToList();

            // 4. Summarize (first few chunks only)
            var snippet = string.Join("\n\n", cleanChunks.Take(3).Select(c => c.Text));
            var description = await _llm.SummarizeAsync(
                "Summarize this document in 1–2 sentences.",
                snippet
            );

            // 5. Create DocumentEntity
            var doc = new DocumentEntity
            {
                Id = documentId,
                UserId = userId,
                FileName = fileName,
                Checksum = checksum,
                Description = description,
                UploadedAt = DateTime.UtcNow,
                PageCount = pageCount,
                ChunkCount = 0
            };

            // 6. Embed chunks (using your existing embedding service)
            var embeddedChunks = await _embedding.EmbedChunksAsync(cleanChunks);

            // 7. Convert to ChunkEntity for persistence
            var chunkEntities = embeddedChunks.Select(ec => new ChunkEntity
            {
                DocumentId = doc.Id,
                Index = ec.Index,
                Text = ec.Text,
                Embedding = ec.Embedding
            });

            // 8. Persist document + chunks
            await _persistence.SaveDocumentAsync(doc, chunkEntities);

            return new DocumentProcessingResult(doc, chunkEntities);
        }
    }
}