using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Calory.Api.Features.FoodEntries;
using Calory.Domain;
using Calory.Domain.Enums;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using UglyToad.PdfPig;

namespace Calory.Api.Features.FoodEntries.ImportFoodEntries;

public sealed class ImportFoodEntriesEndpoint(IUnitOfWork unitOfWork) : Endpoint<ImportFoodEntriesRequest, ImportFoodEntriesResponse>
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly Regex RowPattern = new(@"^(?<date>\d{1,4}[-/.]\d{1,2}[-/.]\d{1,4}|[A-Za-z]{3,9}\s+\d{1,2}(?:,\s*\d{4}|\s+\d{4}))\s+(?:(?<meal>Breakfast|Lunch|Dinner|Snack)\s+)?(?<food>.+?)\s+(?<calories>\d+(?:[.,]\d+)?)\s+(?<protein>\d+(?:[.,]\d+)?)\s+(?<carbs>\d+(?:[.,]\d+)?)\s+(?<fat>\d+(?:[.,]\d+)?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override void Configure()
    {
        Post("/api/food-entries/import-pdf");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
        Summary(summary =>
        {
            summary.Summary = "Import food entries from a PDF food diary";
            summary.Description = "Parses tabular PDF rows containing date, optional meal type, food name, calories, protein, carbohydrates, and fat.";
            summary.Response<ImportFoodEntriesResponse>(200, "Import results including imported and skipped rows.");
            summary.Response(400, "The upload is not a supported PDF or is too large.");
        });
    }

    public override async Task HandleAsync(ImportFoodEntriesRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        if (request.File is null || request.File.Length == 0 || request.File.Length > MaxFileSize ||
            !string.Equals(Path.GetExtension(request.File.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            AddError("Upload a PDF file up to 10 MB.");
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        var skipped = new List<ImportSkippedRow>();
        var imported = new List<FoodEntry>();
        await using var stream = request.File.OpenReadStream();
        using var document = PdfDocument.Open(stream);
        var rowNumber = 0;

        foreach (var page in document.GetPages())
        {
            foreach (var rawLine in page.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                rowNumber++;
                var line = Regex.Replace(rawLine, @"\s+", " ").Trim();
                if (IsHeader(line))
                    continue;

                if (!TryParseRow(line, out var parsed, out var reason))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        skipped.Add(new ImportSkippedRow(rowNumber, line, reason));
                    continue;
                }

                imported.Add(new FoodEntry
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    MealType = parsed.MealType,
                    FoodName = parsed.FoodName,
                    Quantity = 1,
                    Unit = "serving",
                    ConsumedAt = parsed.ConsumedAt,
                    Source = FoodEntrySource.Database,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Nutrition = new FoodNutrition
                    {
                        Id = Guid.NewGuid(),
                        Calories = parsed.Calories,
                        ProteinG = parsed.ProteinG,
                        CarbohydratesG = parsed.CarbohydratesG,
                        FatG = parsed.FatG
                    }
                });
            }
        }

        foreach (var entry in imported)
            unitOfWork.FoodEntries.Add(entry);
        if (imported.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        await Send.OkAsync(new ImportFoodEntriesResponse(
            imported.Count,
            skipped.Count,
            imported.Select(FoodEntryResponse.From).ToList(),
            skipped.Take(50).ToList()), cancellationToken);
    }

    private static bool IsHeader(string line) =>
        line.Contains("calorie", StringComparison.OrdinalIgnoreCase) &&
        (line.Contains("protein", StringComparison.OrdinalIgnoreCase) || line.Contains("food", StringComparison.OrdinalIgnoreCase));

    private static bool TryParseRow(string line, out ParsedRow row, out string reason)
    {
        row = default;
        reason = "The row does not match the supported tabular format.";
        var match = RowPattern.Match(line);
        if (!match.Success)
            return false;

        if (!TryParseDate(match.Groups["date"].Value, out var consumedAt))
        {
            reason = "The date could not be read.";
            return false;
        }

        if (!TryDecimal(match.Groups["calories"].Value, out var calories) ||
            !TryDecimal(match.Groups["protein"].Value, out var protein) ||
            !TryDecimal(match.Groups["carbs"].Value, out var carbs) ||
            !TryDecimal(match.Groups["fat"].Value, out var fat) ||
            calories < 0 || protein < 0 || carbs < 0 || fat < 0)
        {
            reason = "Nutrition values must be non-negative numbers.";
            return false;
        }

        var mealText = match.Groups["meal"].Value;
        var mealType = Enum.TryParse<MealType>(mealText, true, out var parsedMeal) ? parsedMeal : MealType.Snack;
        var foodName = match.Groups["food"].Value.Trim();
        if (foodName.Length == 0 || foodName.Length > 200)
        {
            reason = "The food name is missing or too long.";
            return false;
        }

        row = new ParsedRow(foodName, mealType, consumedAt, calories, protein, carbs, fat);
        return true;
    }

    private static bool TryParseDate(string value, out DateTime result)
    {
        var formats = new[] { "yyyy-MM-dd", "MM/dd/yyyy", "M/d/yyyy", "dd/MM/yyyy", "d/M/yyyy", "MMM d, yyyy", "MMMM d, yyyy", "MMM d yyyy", "MMMM d yyyy" };
        return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result) &&
            (result = DateTime.SpecifyKind(result, DateTimeKind.Utc)) != default;
    }

    private static bool TryDecimal(string value, out decimal result) =>
        decimal.TryParse(value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private readonly record struct ParsedRow(string FoodName, MealType MealType, DateTime ConsumedAt, decimal Calories, decimal ProteinG, decimal CarbohydratesG, decimal FatG);
}

public sealed class ImportFoodEntriesRequest
{
    public IFormFile File { get; set; } = null!;
}

public sealed record ImportFoodEntriesResponse(
    int ImportedCount,
    int SkippedCount,
    IReadOnlyList<FoodEntryResponse> ImportedEntries,
    IReadOnlyList<ImportSkippedRow> SkippedRows);

public sealed record ImportSkippedRow(int RowNumber, string Content, string Reason);
