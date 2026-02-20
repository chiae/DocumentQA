namespace DocumentQA.Models
{
    public class ChunkEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DocumentId { get; set; } = default!;
        public int Index { get; set; }   // chunk number within the document
        public string Text { get; set; } = default!;
        public float[] Embedding { get; set; } = default!;

    }
}
