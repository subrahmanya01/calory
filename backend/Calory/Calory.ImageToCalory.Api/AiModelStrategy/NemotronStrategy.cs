using Calory.ImageToCalory.Api.Clients;
using System.Text.Json;

namespace Calory.ImageToCalory.Api.AiModelStrategy
{
    public class NemotronStrategy : IAiModelStrategy
    {
        private readonly NvidiaClient _nvidiaClient;
        private readonly IConfiguration _configuration;

        public string ModelName => "nvidia/nemotron-3-nano-omni-30b-a3b-reasoning";

        public NemotronStrategy(NvidiaClient nvidiaClient, IConfiguration configuration)
        {
            _nvidiaClient = nvidiaClient;
            _configuration = configuration;
        }

        public async Task<string> AnalyzeImageAsync( IFormFile image, string? prompt, CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();

            await image.CopyToAsync( memoryStream, cancellationToken);

            var base64 = Convert.ToBase64String(memoryStream.ToArray());

            var imageUrl = $"data:{image.ContentType};base64,{base64}";

            var payload = new
            {
                model = ModelName,

                messages = new[]
                {
                new
                {
                    role = "user",

                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = prompt ??
                                   "What is in this image?"
                        },

                        new
                        {
                            type = "image_url",

                            image_url = new
                            {
                                url = imageUrl
                            }
                        }
                    }
                }
            },

                max_tokens = 65536,
                reasoning_budget = 16384,
                stream = false,
                temperature = 0.6,
                top_p = 0.95
            };

            var apiKey = _configuration.GetValue<string>("ApiKeys:Nemotron") ?? throw new InvalidOperationException("API key for KimiK3 is not configured");
            var response = await _nvidiaClient.ChatCompletionAsync( payload, apiKey, cancellationToken);

            return ExtractResult(response);
        }

        private static string ExtractResult(
            JsonDocument response)
        {
            return response
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
    }
}
