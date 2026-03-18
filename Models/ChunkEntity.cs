namespace DocumentQA.Models
{
    /// <summary>
    /// Represents a single embedded text chunk belonging to a document.
    /// Each chunk stores the raw text segment, its position within the
    /// document, and the vector embedding used for semantic search.
    ///
    /// This entity is created during the document ingestion pipeline,
    /// persisted in the database, and later retrieved for similarity
    /// search during question‑answering.
    /// </summary>
    public class ChunkEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Foreign key linking this chunk to its parent document.
        /// </summary>
        public string DocumentId { get; set; } = default!;

        /// <summary>
        /// Navigation property to the parent document.
        /// </summary>
        public DocumentEntity Document { get; set; } = default!;

        /// <summary>
        /// The chunk's sequential index within the document (0‑based).
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The raw text content of this chunk.
        /// </summary>
        public string Text { get; set; } = default!;

        /// <summary>
        /// The vector embedding representing the semantic meaning of the chunk.
        /// Stored as a float array and used for similarity search.
        /// </summary>
        public float[] Embedding { get; set; } = default!;
    }
}