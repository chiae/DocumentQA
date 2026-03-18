namespace DocumentQA.Models
{
    public class DocumentProcessingResult
    {
        public DocumentEntity Document { get; }
        public IEnumerable<ChunkEntity> Chunks { get; }

        public DocumentProcessingResult(DocumentEntity doc, IEnumerable<ChunkEntity> chunks)
        {
            Document = doc;
            Chunks = chunks;
        }
    }
}
