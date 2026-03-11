using DocumentQA.Models;
using System.Text;
using System.Text.Json;

namespace DocumentQA.Services
{
    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _http;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _deployment;

        public AzureOpenAIEmbeddingService(IConfiguration config)
        {
            _http = new HttpClient();
            _endpoint = config["AzureOpenAI:Endpoint"];
            _apiKey = config["AzureOpenAI:ApiKey"];
            _deployment = config["AzureOpenAI:EmbeddingDeployment"];
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var requestBody = new { input = text };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_endpoint}/openai/deployments/{_deployment}/embeddings?api-version=2025-01-01-preview"
            );

            request.Headers.Add("api-key", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

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