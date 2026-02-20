
namespace DocumentQA.Services
{
    public class OllamaLmService : ILlmService
    {
        private readonly HttpClient _http;

        public OllamaLmService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> AskAsync(string question, string context)
        {
            var prompt = $"{context}\n\nQuestion: {question}";

            var request = new
            {
                model = "llama3",   // or mistral, phi3, etc.
                prompt = prompt,
                stream = false
            };

            var response = await _http.PostAsJsonAsync("http://localhost:11434/api/generate", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

            return result?.Response ?? "";
        }
    }

    public class OllamaResponse
    {
        public string Response { get; set; }
    }
}