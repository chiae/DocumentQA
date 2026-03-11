using DocumentQA.Models;
using System.Text;
using System.Text.Json;

namespace DocumentQA.Services
{
    public class GroqEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GroqEmbeddingService(IConfiguration config)
        {
            _http = new HttpClient();
            _apiKey = config["Groq:ApiKey"];
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var requestBody = new
            {
                model = "nomic-embed-text",
                input = text
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/embeddings"
            );

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("Accept", "application/json");

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq embedding error: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var embedding = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return embedding;
        }

        public async Task<List<ChunkEntity>> EmbedChunksAsync(List<TextChunk> chunks)
        {
            var list = new List<ChunkEntity>();

            foreach (var chunk in chunks)
            {
                var embedding = await GenerateEmbeddingAsync(chunk.Text);

                list.Add(new ChunkEntity
                {
                    DocumentId = chunk.DocumentId,
                    Index = chunk.Index,
                    Text = chunk.Text,
                    Embedding = embedding
                });
            }

            return list;
        }
    }
}