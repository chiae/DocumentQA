using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace DocumentQA.Services
{
    public class AzureOpenAiLlmService : ILlmService
    {
        private readonly ChatClient _chat;

        public AzureOpenAiLlmService(IConfiguration config)
        {
            var endpoint = config["AzureOpenAI:Endpoint"];
            var key = config["AzureOpenAI:ApiKey"];
            var deployment = config["AzureOpenAI:DeploymentName"];

            var client = new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(key)
            );
            Console.WriteLine($"ENDPOINT: '{endpoint}'");
            Console.WriteLine($"DEPLOYMENT: '{deployment}'");
            Console.WriteLine($"KEY LENGTH: {key?.Length}");


            _chat = client.GetChatClient(deployment);
        }

        public async Task<string> AskAsync(string question, string context)
        {
            var prompt = $"{context}\n\nQuestion: {question}";

            var result = await _chat.CompleteChatAsync(prompt);

            return result.Value.Content[0].Text ?? "";
        }
    }
}