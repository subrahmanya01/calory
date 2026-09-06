using Calory.ImageToCalory.Api.AiModelStrategy;
using Calory.ImageToCalory.Api.Models;

namespace Calory.ImageToCalory.Api.Services
{
    public class AiService : IAiService
    {
        private readonly IAiModelStrategyResolver _resolver;

        public AiService( IAiModelStrategyResolver resolver)
        {
            _resolver = resolver;
        }

        public async Task<AnalyzeImageResponse> AnalyzeImageAsync(AnalyzeImageRequest request, CancellationToken cancellationToken = default)
        {
            var strategy =
                _resolver.Resolve(request.Model);

            var result = await strategy.AnalyzeImageAsync(request.Image, request.Prompt, cancellationToken);

            return new AnalyzeImageResponse
            {
                Success = true,
                Model = strategy.ModelName,
                Result = result
            };
        }
    }
}