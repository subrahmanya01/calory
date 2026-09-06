using System.ComponentModel;
using System.Text.Json;
using Calory.Api.Features.ImageAnalysis.FoodAnalysis;
using Google.GenAI;
using Google.GenAI.Types;
using ModelContextProtocol.Server;

namespace Calory.Api.Mcp;

[McpServerToolType]
public sealed class ImageAnalysisTools(IConfiguration configuration)
{
    private static readonly string PromptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", "ImageToCalory.md");
    private static string? prompt;

    [McpServerTool, Description("Analyzes a food image and returns an estimated dish, portion size, calories, and nutrition. Pass the image as base64 without a data URL prefix.")]
    public async Task<string> AnalyzeFoodImage(
        [Description("Base64-encoded image bytes, up to 10 MB.")] string imageBase64,
        [Description("Image MIME type, for example image/jpeg or image/png.")] string contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageBase64) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A valid base64 image and image MIME type are required.");

        byte[] imageData;
        try
        {
            imageData = Convert.FromBase64String(imageBase64);
        }
        catch (FormatException)
        {
            throw new ArgumentException("The image must be valid base64.");
        }

        if (imageData.Length == 0 || imageData.Length > 10 * 1024 * 1024)
            throw new ArgumentException("The image must be between 1 byte and 10 MB.");

        var client = new Client(apiKey: configuration["Gemini:ApiKey"]);
        var response = await client.Models.GenerateContentAsync(
            model: "gemini-2.5-flash",
            contents: new List<Content>
            {
                new Content
                {
                    Role = "user",
                    Parts = new List<Part>
                    {
                        new Part { Text = await GetPromptAsync(cancellationToken) },
                        new Part { InlineData = new Blob { Data = imageData, MimeType = contentType } }
                    }
                }
            },
            config: new GenerateContentConfig { ResponseMimeType = "application/json", Temperature = 0.2f });

        var json = response.Text?.Trim();
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("The food image analysis returned no result.");

        try
        {
            var result = JsonSerializer.Deserialize<FoodAnalysisResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result is null
                ? throw new InvalidOperationException("The food image analysis returned an invalid result.")
                : JsonSerializer.Serialize(result);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("The food image analysis returned invalid JSON.", exception);
        }
    }

    private static async Task<string> GetPromptAsync(CancellationToken cancellationToken)
    {
        if (prompt is not null)
            return prompt;

        prompt = await System.IO.File.ReadAllTextAsync(PromptPath, cancellationToken);
        return prompt;
    }
}