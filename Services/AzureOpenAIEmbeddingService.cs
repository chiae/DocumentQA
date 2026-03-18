using DocumentQA.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentQA.Services
{
    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _http;
        private readonly string _endpoint;
        private readonly string _deployment;
        private readonly string _apiKey;


        public AzureOpenAIEmbeddingService(IConfiguration config)
        {
            _endpoint = config["AzureOpenAI:Endpoint"];
            // Example: https://echia-openai-resource.openai.azure.com/

            _deployment = config["AzureOpenAI:EmbeddingDeployment"];
            // Example: Cohere-embed-v3-english

            _apiKey = config["AzureOpenAI:ApiKey"];

            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("api-key", _apiKey);
        }

        private string BuildUrl() =>
            $"{_endpoint}openai/deployments/{_deployment}/embeddings?api-version=2023-05-15";

        // ------------------------------------------------------------
        // PUBLIC INTERFACE IMPLEMENTATION
        // ------------------------------------------------------------
        async Task<float[]> IEmbeddingService.GenerateEmbeddingAsync(string text)
        {
            return await GenerateEmbeddingInternalAsync(text);
        }

        async Task<List<ChunkEntity>> IEmbeddingService.EmbedChunksAsync(List<TextChunk> chunks)
        {
            return await EmbedChunksInternalAsync(chunks);
        }

        // ------------------------------------------------------------
        // INTERNAL IMPLEMENTATION
        // ------------------------------------------------------------
        private async Task<float[]> GenerateEmbeddingInternalAsync(string text)
        {
            var payload = new
            {
                input = new[] { text }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(BuildUrl(), content);
            response.EnsureSuccessStatusCode();

            // Capture raw JSON for debugging
            var rawJson = await response.Content.ReadAsStringAsync();


            using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<EmbeddingResponse>(stream);
            return result?.Data?[0]?.Embedding ?? Array.Empty<float>();
        }

        private async Task<List<ChunkEntity>> EmbedChunksInternalAsync(List<TextChunk> chunks)
        {
            var texts = chunks.Select(c => c.Text).ToArray();

            var payload = new
            {
                input = texts
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(BuildUrl(), content);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<EmbeddingResponse>(stream);

            var vectors = result?.Data?.Select(d => d.Embedding).ToArray() ?? Array.Empty<float[]>();

            var list = new List<ChunkEntity>();

            for (int i = 0; i < chunks.Count; i++)
            {
                list.Add(new ChunkEntity
                {
                    DocumentId = chunks[i].DocumentId,
                    Index = chunks[i].Index,
                    Text = chunks[i].Text,
                    Embedding = vectors[i]
                });
            }

            return list;
        }
    }

    // ------------------------------------------------------------
    // RESPONSE MODELS
    // ------------------------------------------------------------
    public class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingItem> Data { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("usage")]
        public UsageItem Usage { get; set; }

    }

    public class EmbeddingItem
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; }

    }

    public class UsageItem
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

}