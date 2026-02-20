using OpenAI;

namespace DocumentQA.Services
{
    public class OpenAiLmService : ILlmService
    {
        private readonly OpenAIClient _client;

        public OpenAiLmService(OpenAIClient client)
        {
            _client = client;
        }

        public async Task<string> AskAsync(string question, string context)
        {
            var chat = _client.GetChatClient("gpt-4o-mini");

            // This SDK version uses a single string prompt, not message objects
            var prompt = $"{context}\n\nQuestion: {question}";

            var result = await chat.CompleteChatAsync(prompt);

            // The actual ChatCompletion is in result.Value
            var completion = result.Value;

            // The text is in completion.OutputText
            return completion.Content.First().Text ?? String.Empty;

        }
    }
}