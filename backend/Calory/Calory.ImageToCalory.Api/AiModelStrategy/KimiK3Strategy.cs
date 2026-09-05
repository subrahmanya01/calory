using Calory.ImageToCalory.Api.Clients;
using System.Text.Json;

namespace Calory.ImageToCalory.Api.AiModelStrategy
{
    public class KimiK3Strategy : IAiModelStrategy
    {
        private readonly NvidiaClient _nvidiaClient;
        private readonly IConfiguration _configuration;

        public string ModelName => "moonshotai/kimi-k3";

        public KimiK3Strategy(NvidiaClient nvidiaClient, IConfiguration configuration)
        {
            _nvidiaClient = nvidiaClient;
            _configuration = configuration;
        }

        public async Task<string> AnalyzeImageAsync( IFormFile image, string? prompt, CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();

            await image.CopyToAsync(memoryStream, cancellationToken);
            var base64 = Convert.ToBase64String(memoryStream.ToArray());
            var imageUrl = $"data:{image.ContentType};base64,{base64}";

            var payload = new
            {
                model = ModelName,

                messages = new[] {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = prompt ?? "What is in this image?"
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

                max_tokens = 16384,
                seed = 0,
                stream = false,
                temperature = 1,
                reasoning_effort = "max"
            };

            var apiKey = _configuration.GetValue<string>("ApiKeys:KimiK3") ?? throw new InvalidOperationException("API key for KimiK3 is not configured");

            var response = await _nvidiaClient.ChatCompletionAsync(payload, apiKey, cancellationToken);

            return ExtractResult(response);
        }

        private static string ExtractResult(JsonDocument response)
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
