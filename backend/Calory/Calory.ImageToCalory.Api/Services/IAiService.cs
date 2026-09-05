using Calory.ImageToCalory.Api.Models;

namespace Calory.ImageToCalory.Api.Services
{
    public interface IAiService
    {
        Task<AnalyzeImageResponse> AnalyzeImageAsync(
            AnalyzeImageRequest request,
            CancellationToken cancellationToken = default);
    }
}
