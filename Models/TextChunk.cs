namespace DocumentQA.Models
{
    /// <summary>
    /// Represents a raw text chunk produced during the initial document
    /// ingestion phase, before embeddings are generated.
    ///
    /// This lightweight model is used to hold the plain text segments and
    /// their sequential order within the document. It is typically passed
    /// into the embedding pipeline, which transforms each chunk into a
    /// <see cref="ChunkEntity"/> with vector embeddings for semantic search.
    /// </summary>
    public class TextChunk
    {
        /// <summary>
        /// Identifier of the document this chunk belongs to.
        /// </summary>
        public string DocumentId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The chunk's sequential index within the document (0‑based).
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The raw text content of this chunk.
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}