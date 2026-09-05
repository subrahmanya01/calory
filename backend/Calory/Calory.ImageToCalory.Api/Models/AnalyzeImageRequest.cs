namespace Calory.ImageToCalory.Api.Models
{
    public class AnalyzeImageRequest
    {
        public string Model { get; set; } = string.Empty;

        public string? Prompt { get; set; }

        public IFormFile Image { get; set; } = null!;
    }
}
