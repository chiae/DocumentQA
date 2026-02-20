    

namespace DocumentQA.Services
{
    public class RagService : IRagService
    {
        private readonly IRetrievalService _retrieval;
        private readonly ILlmService _llm;
        public RagService(IRetrievalService retrieval, ILlmService llm)
        {
            _retrieval = retrieval;
            _llm = llm;
        }

        async Task<string> IRagService.AskAsync(string question, string? documentId)
        {
            // Retrieve the top chunks
            var chunks = await _retrieval.RetrieveRelevantChunksAsync(question,documentId,topK:10);

            // Build the context
            var context = String.Join("\n\n", chunks.Select(c => c.Text));

            var answer = await _llm.AskAsync(question, context);
            return answer;

        }
    }
}
 