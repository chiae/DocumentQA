namespace DocumentQA.Models
{
    /// <summary>
    /// Represents an uploaded document within the system, including its
    /// metadata, ownership, and the collection of text chunks generated
    /// during ingestion.
    ///
    /// This entity is created when a user uploads a file. It stores basic
    /// information such as the file name, upload timestamp, page count,
    /// and the number of generated chunks. The associated chunks contain
    /// the embedded text segments used for semantic search and Q&A.
    /// </summary>
    public class DocumentEntity
    {
        /// <summary>
        /// Unique identifier for the document.
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// Original file name of the uploaded document.
        /// </summary>
        public string FileName { get; set; } = default!;
        public string Checksum { get; set; }

        /// <summary>
        /// Optional user-provided description of the document.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Timestamp indicating when the document was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Number of pages detected in the document, if applicable.
        /// </summary>
        public int? PageCount { get; set; }

        /// <summary>
        /// Number of text chunks generated during ingestion.
        /// </summary>
        public int? ChunkCount { get; set; }

        /// <summary>
        /// Collection of text chunks belonging to this document.
        /// </summary>
        public ICollection<ChunkEntity> Chunks { get; set; } = new List<ChunkEntity>();

        /// <summary>
        /// Identifier of the user who owns this document.
        /// </summary>
        public string? UserId { get; internal set; }
    }
}