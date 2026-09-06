using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using Calory.Api.Features.Reports;
using Calory.Persistance.Interfaces;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Calory.Api.Mcp;

[McpServerToolType]
public sealed class ReportTools(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
{
    [McpServerTool, Description("Gets daily calorie, macro, fiber, and entry totals for a date range. Dates use YYYY-MM-DD; omitted dates default to the last seven days.")]
    public async Task<string> GetDailyNutritionReport(
        [Description("Optional inclusive start date in YYYY-MM-DD format.")] string? from = null,
        [Description("Optional inclusive end date in YYYY-MM-DD format.")] string? to = null,
        CancellationToken cancellationToken = default)
    {
        var entries = await GetEntries(from, to, cancellationToken);
        var result = entries
            .GroupBy(entry => DateOnly.FromDateTime(entry.ConsumedAt.ToUniversalTime()))
            .OrderBy(group => group.Key)
            .Select(group => new DailyNutritionResponse(group.Key, NutritionTotals.From(group)))
            .ToList();
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Gets daily calorie and macro trend points for a date range. Dates use YYYY-MM-DD; omitted dates default to the last seven days.")]
    public async Task<string> GetNutritionTrends(
        [Description("Optional inclusive start date in YYYY-MM-DD format.")] string? from = null,
        [Description("Optional inclusive end date in YYYY-MM-DD format.")] string? to = null,
        CancellationToken cancellationToken = default)
    {
        var entries = await GetEntries(from, to, cancellationToken);
        var result = entries
            .GroupBy(entry => DateOnly.FromDateTime(entry.ConsumedAt.ToUniversalTime()))
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var totals = NutritionTotals.From(group);
                return new TrendPoint(group.Key, totals.Calories, totals.ProteinG, totals.CarbohydratesG, totals.FatG, totals.EntryCount);
            })
            .ToList();
        return JsonSerializer.Serialize(result);
    }

    private async Task<IReadOnlyList<Calory.Domain.FoodEntry>> GetEntries(string? from, string? to, CancellationToken cancellationToken)
    {
        var start = ParseDate(from, DateTime.UtcNow.Date.AddDays(-6));
        var end = ParseDate(to, DateTime.UtcNow.Date).AddDays(1);
        if (end <= start)
            throw new ArgumentException("The end date must be on or after the start date.");

        return await unitOfWork.FoodEntries.GetByUserAndRangeAsync(GetUserId(), start, end, cancellationToken);
    }

    private static DateTime ParseDate(string? value, DateTime fallback) =>
        string.IsNullOrWhiteSpace(value)
            ? fallback
            : DateOnly.TryParse(value, out var date)
                ? date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                : throw new ArgumentException("Dates must use YYYY-MM-DD format.");

    private Guid GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("A valid authenticated user is required.");
    }
}