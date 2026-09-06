using System.Text.Json;
using FastEndpoints;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.ImageAnalysis.FoodAnalysis;

public sealed class FoodAnalysisEndpoint : Endpoint<FoodAnalysisRequest, FoodAnalysisResponse>
{
    private static readonly string _promptPath = Path.Combine( AppContext.BaseDirectory, "Prompts", "ImageToCalory.md");
    private static string? _prompt = null;
    private readonly IConfiguration _configuration;
    public FoodAnalysisEndpoint(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void Configure()
    {
        Post("/api/image-analysis/food");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
        Summary(summary =>
        {
            summary.Summary = "Analyze food from an image";
            summary.Description = "Uses Gemini 2.5 Flash to estimate the dish, portion size, calories, and macronutrients.";
            summary.Response<FoodAnalysisResponse>(200, "The structured nutrition estimate.");
            summary.Response(400, "A valid image upload is required.");
            summary.Response(401, "A valid JWT is required.");
            summary.Response(502, "The Gemini analysis failed or returned invalid JSON.");
        });
    }

    public override async Task HandleAsync(FoodAnalysisRequest request, CancellationToken cancellationToken)
    {
        var image = request.Image;
        if (image is null || image.Length == 0 || string.IsNullOrWhiteSpace(image.ContentType) || !image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        if (image.Length > 10 * 1024 * 1024)
        {
            AddError("The image must be 10 MB or smaller.");
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        try
        {
            var prompt = await GetPromptAsync(cancellationToken);
            await using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream, cancellationToken);

            var client = new Client(apiKey: _configuration["Gemini:ApiKey"]);
            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash",
                contents: new List<Content>
                {
                    new Content
                    {
                        Role = "user",
                        Parts = new List<Part>
                        {
                            new Part { Text = prompt },
                            new Part
                            {
                                InlineData = new Blob
                                {
                                    Data = memoryStream.ToArray(),
                                    MimeType = image.ContentType
                                }
                            }
                        }
                    }
                },
                config: new GenerateContentConfig
                {
                    ResponseMimeType = "application/json",
                    Temperature = 0.2f
                });

            var json = response.Text?.Trim();
            if (string.IsNullOrWhiteSpace(json))
            {
                await Send.StatusCodeAsync(502, cancellationToken);
                return;
            }

            var nutritionData = JsonSerializer.Deserialize<FoodAnalysisResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (nutritionData is null)
            {
                await Send.StatusCodeAsync(502, cancellationToken);
                return;
            }

            await Send.OkAsync(nutritionData, cancellationToken);
        }
        catch (JsonException)
        {
            await Send.StatusCodeAsync(502, cancellationToken);
        }
        catch (Exception)
        {
            await Send.StatusCodeAsync(502, cancellationToken);
        }
    }

    private async Task<string> GetPromptAsync(CancellationToken cancellationToken)
    {
        if(_prompt != null)
        {
            return _prompt;
        }

        var prompt = await System.IO.File.ReadAllTextAsync(_promptPath, cancellationToken);
        _prompt = prompt;
        return prompt;
    }
}
