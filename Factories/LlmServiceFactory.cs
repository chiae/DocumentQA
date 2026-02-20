using DocumentQA.Services;

namespace DocumentQA.Factories
{
    public class LlmServiceFactory
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public LlmServiceFactory(IServiceProvider services, IConfiguration config)
        {
            _services = services;
            _config = config;
        }

        public ILlmService Create()
        {
            var provider = _config["LlmProvider"]?.ToLowerInvariant();

            return provider switch
            {
                "azure" => _services.GetRequiredService<AzureOpenAiLlmService>(),
                "openai" => _services.GetRequiredService<OpenAiLmService>(),
                "ollama" => _services.GetRequiredService<OllamaLmService>(),
                _ => throw new Exception("Invalid LlmProvider value")
            };
        }
    }
}
