using DocumentQA.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentQA.Factories
{
    public class EmbeddingServiceFactory
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public EmbeddingServiceFactory(IServiceProvider services, IConfiguration config)
        {
            _services = services;
            _config = config;
        }

        public IEmbeddingService Create()
        {
            var provider = _config["EmbeddingProvider"]?.ToLowerInvariant();

            return provider switch
            {
                "azure" => _services.GetRequiredService<AzureOpenAIEmbeddingService>(),
                //"openai" => _services.GetRequiredService<OpenAIEmbeddingService>(),
                "groq" => _services.GetRequiredService<GroqEmbeddingService>(),
                //"ollama" => _services.GetRequiredService<OllamaEmbeddingService>(),

                _ => throw new Exception($"Unknown embedding provider: {provider}")
            };
        }
    }
}