namespace Calory.Api.Features.ImageAnalysis.FoodAnalysis;

public sealed class FoodAnalysisRequest
{
    public IFormFile Image { get; set; } = null!;
}
