using OpenAI.Chat;

namespace DocumentQA.Services
{
    public class AzureOpenAiLlmService(ChatClient chat) : ILlmService
    {
        private readonly ChatClient _chat = chat;

        public async Task<string> AskAsync(string question, string context)
        {
            var messages = new List<ChatMessage>
                    {
                        ChatMessage.CreateSystemMessage(
                    "Respond using GitHub-flavored Markdown. Do NOT wrap your answer in code fences. Return raw Markdown only."
                ),

                        ChatMessage.CreateUserMessage(
                            $"{context}\n\nQuestion: {question}"
                        )
                    };

            var result = await _chat.CompleteChatAsync(messages);
            return result.Value.Content[0].Text ?? "";
        }

        public async Task<string> SummarizeAsync(string question, string text)
        {
            var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(
                        "You are a summarization assistant. Produce a concise 1–2 sentence summary. " +
                        "Respond using plain text only."
                    ),

                    ChatMessage.CreateUserMessage(
                        $"{question}:\n\n{text}"
                    )
                };

            var result = await _chat.CompleteChatAsync(messages);
            return result.Value.Content[0].Text ?? "";
        }

    }
}