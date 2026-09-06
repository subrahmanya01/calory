using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using Calory.Persistance.Interfaces;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Calory.Api.Mcp;

[McpServerToolType]
public sealed class NutritionTools(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
{
    [McpServerTool, Description("Gets the authenticated user's food entries for a date range. Dates use YYYY-MM-DD format.")]
    public async Task<string> GetFoodEntries(
        [Description("Inclusive start date in YYYY-MM-DD format.")] string from,
        [Description("Inclusive end date in YYYY-MM-DD format.")] string to,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = ParseRange(from, to);
        var entries = await unitOfWork.FoodEntries.GetByUserAndRangeAsync(GetUserId(), start, end, cancellationToken);
        return JsonSerializer.Serialize(entries.Select(entry => new
        {
            entry.FoodName,
            entry.MealType,
            entry.Quantity,
            entry.Unit,
            entry.ConsumedAt,
            Nutrition = new { entry.Nutrition.Calories, entry.Nutrition.ProteinG, entry.Nutrition.CarbohydratesG, entry.Nutrition.FatG }
        }));
    }

    [McpServerTool, Description("Calculates the authenticated user's calorie and macro totals for a date range.")]
    public async Task<string> GetNutritionSummary(
        [Description("Inclusive start date in YYYY-MM-DD format.")] string from,
        [Description("Inclusive end date in YYYY-MM-DD format.")] string to,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = ParseRange(from, to);
        var entries = await unitOfWork.FoodEntries.GetByUserAndRangeAsync(GetUserId(), start, end, cancellationToken);
        return JsonSerializer.Serialize(new
        {
            Calories = entries.Sum(entry => entry.Nutrition.Calories),
            ProteinG = entries.Sum(entry => entry.Nutrition.ProteinG),
            CarbohydratesG = entries.Sum(entry => entry.Nutrition.CarbohydratesG),
            FatG = entries.Sum(entry => entry.Nutrition.FatG),
            EntryCount = entries.Count
        });
    }

    private Guid GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("A valid authenticated user is required.");
    }

    private static (DateTime Start, DateTime End) ParseRange(string from, string to)
    {
        if (!DateOnly.TryParse(from, out var startDate) || !DateOnly.TryParse(to, out var endDate) || endDate < startDate)
            throw new ArgumentException("Dates must be valid YYYY-MM-DD values and the end date cannot precede the start date.");

        return (
            startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1));
    }
}