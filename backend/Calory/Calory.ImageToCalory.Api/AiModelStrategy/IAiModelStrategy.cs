namespace Calory.ImageToCalory.Api.AiModelStrategy
{
    public interface IAiModelStrategy
    {
        string ModelName { get; }

        Task<string> AnalyzeImageAsync(IFormFile image, string? prompt, CancellationToken cancellationToken = default);
    }
}
