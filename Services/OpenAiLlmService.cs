using OpenAI;

namespace DocumentQA.Services
{
    public class OpenAiLmService : ILlmService
    {
        private readonly OpenAIClient _client;
        private readonly string _model;

        public OpenAiLmService(IConfiguration config)
        {
            _client = new OpenAIClient(config["OpenAI:ApiKey"]);
            _model = config["OpenAI:Model"];
        }

        public async Task<string> AskAsync(string question, string context)
        {
            var prompt = $"{context}\n\nQuestion: {question}";

            var chat = _client.GetChatClient(_model);

            var result = await chat.CompleteChatAsync(prompt);

            return result.Value.Content[0].Text ?? "";
        }
    }
}