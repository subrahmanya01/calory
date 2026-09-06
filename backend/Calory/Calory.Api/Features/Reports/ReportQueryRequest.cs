namespace Calory.Api.Features.Reports;

public sealed class ReportQueryRequest
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    public (DateTime From, DateTime To)? ToUtcRange()
    {
        var from = From?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? DateTime.UtcNow.Date.AddDays(-6);
        var to = To?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1) ?? DateTime.UtcNow.Date.AddDays(1);
        return to > from ? (from, to) : null;
    }
}

public sealed record NutritionTotals(
    decimal Calories,
    decimal ProteinG,
    decimal CarbohydratesG,
    decimal FatG,
    decimal FiberG,
    int EntryCount)
{
    public static NutritionTotals From(IEnumerable<Calory.Domain.FoodEntry> entries)
    {
        var list = entries.ToList();
        return new(
            list.Sum(entry => entry.Nutrition.Calories),
            list.Sum(entry => entry.Nutrition.ProteinG),
            list.Sum(entry => entry.Nutrition.CarbohydratesG),
            list.Sum(entry => entry.Nutrition.FatG),
            list.Sum(entry => entry.Nutrition.FiberG),
            list.Count);
    }
}

public sealed record DailyNutritionResponse(DateOnly Date, NutritionTotals Totals);

public sealed record TrendPoint(
    DateOnly Date,
    decimal Calories,
    decimal ProteinG,
    decimal CarbohydratesG,
    decimal FatG,
    int EntryCount);