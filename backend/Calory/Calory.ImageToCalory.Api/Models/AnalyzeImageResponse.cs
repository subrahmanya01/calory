namespace Calory.ImageToCalory.Api.Models
{
    public class AnalyzeImageResponse
    {
        public bool Success { get; set; }

        public string Model { get; set; } = string.Empty;

        public string Result { get; set; } = string.Empty;
    }
}
