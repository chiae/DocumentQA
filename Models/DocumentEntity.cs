namespace DocumentQA.Models
{
    public class DocumentEntity
    {
        public string Id { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int? PageCount { get; set; }
        public int? ChunkCount { get; set; }

        public ICollection<ChunkEntity> Chunks { get; set; } = new List<ChunkEntity>();
    }

}
