using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Core;
using System.Text;
using System.Text.Json;

namespace DocumentQA.Services
{
    public class OcrService : IOcrService
    {
        private readonly string _endpoint;
        private readonly string _key;

        public OcrService(string endpoint, string key)
        {
            _endpoint = endpoint;
            _key = key;
        }

        public async Task<string> ExtractTextAsync(Stream stream)
        {
            stream.Position = 0;

            // Convert PDF to base64
            var ms = (MemoryStream)stream;
            string base64 = Convert.ToBase64String(ms.ToArray());

            // Build JSON payload
            var payload = new { base64Source = base64 };
            var json = JsonSerializer.Serialize(payload);
            var content = RequestContent.Create(Encoding.UTF8.GetBytes(json));

            var client = new DocumentIntelligenceClient(
                new Uri(_endpoint),
                new AzureKeyCredential(_key)
            );

            // Call protocol API
            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                content
            );

            // Raw JSON
            var resultJson = operation.Value.ToString();

            // Deserialize using our corrected model
            var root = JsonSerializer.Deserialize<RootResult>(
    resultJson,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
);

            // Extract full text
            return root?.AnalyzeResult?.Content ?? string.Empty;
        }
    }

    // -----------------------------
    // JSON MODELS (MATCH EXACTLY)
    // -----------------------------

    public class RootResult
    {
        public string Status { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastUpdatedDateTime { get; set; }
        public AnalyzeResult AnalyzeResult { get; set; }
    }

    public class AnalyzeResult
    {
        public string ApiVersion { get; set; }
        public string ModelId { get; set; }
        public string StringIndexType { get; set; }
        public string Content { get; set; }
        public List<PageResult> Pages { get; set; }
        public List<ParagraphResult> Paragraphs { get; set; }
        public List<object> Styles { get; set; }
        public string ContentFormat { get; set; }
    }

    public class PageResult
    {
        public int PageNumber { get; set; }
        public float Angle { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Unit { get; set; }
        public List<WordResult> Words { get; set; }
        public List<SpanResult> Spans { get; set; }
    }

    public class WordResult
    {
        public string Content { get; set; }
        public List<float> Polygon { get; set; }
        public List<SpanResult> Spans { get; set; }
    }

    public class SpanResult
    {
        public int Offset { get; set; }
        public int Length { get; set; }
    }

    public class ParagraphResult
    {
        public List<SpanResult> Spans { get; set; }
        public List<BoundingRegion> BoundingRegions { get; set; }
        public string Content { get; set; }
    }

    public class BoundingRegion
    {
        public int PageNumber { get; set; }
        public List<float> Polygon { get; set; }
    }
}
