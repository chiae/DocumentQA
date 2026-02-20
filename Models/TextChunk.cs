namespace DocumentQA.Models
{
    public class TextChunk
    {
        public string DocumentId { get; set; } = Guid.NewGuid().ToString();
        public int Index { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}