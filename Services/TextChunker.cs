using System.Text;
using System.Text.RegularExpressions;
using DocumentQA.Models;

namespace DocumentQA.Services
{
    public class TextChunker : ITextChunker
    {
        private readonly int _chunkSize;
        private readonly int _overlap;

        public TextChunker(int chunkSize = 500, int overlap = 100)
        {
            _chunkSize = chunkSize;
            _overlap = overlap;
        }

        public List<TextChunk> Chunk(string text)
        {
            var normalized = Normalize(text);
            var sentences = SplitIntoSentences(normalized);

            var chunks = new List<TextChunk>();
            var current = new StringBuilder();
            int index = 0;
            string documentId = Guid.NewGuid().ToString();

            foreach (var sentence in sentences)
            {
                if (current.Length + sentence.Length > _chunkSize)
                {
                    chunks.Add(new TextChunk
                    {
                        DocumentId = documentId,
                        Index = index++,
                        Text = current.ToString().Trim()
                    });

                    var overlapText = GetOverlap(current.ToString());
                    current = new StringBuilder(overlapText);
                }

                current.Append(sentence);
            }

            if (current.Length > 0)
            {
                chunks.Add(new TextChunk
                {
                    DocumentId = documentId,
                    Index = index,
                    Text = current.ToString().Trim()
                });
            }

            return chunks;
        }

        private string Normalize(string text)
        {
            text = text.Replace("\r", "");
            text = Regex.Replace(text, @"[ \t]+", " ");
            text = Regex.Replace(text, @"\n{2,}", "\n");
            return text.Trim();
        }

        private List<string> SplitIntoSentences(string text)
        {
            var parts = Regex.Split(text, @"(?<=[\.!\?])\s+");
            return parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        }

        private string GetOverlap(string chunk)
        {
            if (chunk.Length <= _overlap)
                return chunk;

            return chunk[^_overlap..];
        }
    }
}