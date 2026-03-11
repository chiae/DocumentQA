using System.Net.Http.Headers;
using System.Text.Json;

namespace DocumentQA.Services
{
    public class GroqLlmService : ILlmService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GroqLlmService(IConfiguration config)
        {
            _http = new HttpClient();
            _apiKey = config["Groq:ApiKey"] ?? throw new Exception("Missing Groq API key");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<string> AskAsync(string question, string context)
        {
            var payload = new
            {
                model = "llama3-70b-8192",
                messages = new[]
    {
        new { role = "system", content = "Respond using GitHub-flavored Markdown. Do NOT wrap your answer in code fences. Return raw Markdown only." },
        new { role = "user", content = $"{context}\n\nQuestion: {question}" }
    }
            };

            var response = await _http.PostAsJsonAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                payload
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq LLM error: {error}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            return json
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }

        public async Task<string> SummarizeAsync(string question, string text)
        {
            var payload = new
            {
                model = "llama3-70b-8192",
                messages = new[]
                {
                    new { role = "system", content = "You are a summarization assistant. Produce a concise 1–2 sentence summary. Respond using plain text only." },
                    new { role = "user", content = $"{question}:\n\n{text}" }
                }
            };

            var response = await _http.PostAsJsonAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                payload
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq LLM error: {error}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            return json
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
    }
}