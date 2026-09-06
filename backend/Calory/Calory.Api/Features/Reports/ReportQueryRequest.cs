namespace Calory.Api.Features.Reports;

public sealed class ReportQueryRequest
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;

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
    decimal SugarG,
    decimal SodiumMg,
    decimal CalciumMg,
    decimal IronMg,
    decimal MagnesiumMg,
    decimal PotassiumMg,
    decimal ZincMg,
    decimal VitaminAMcg,
    decimal VitaminB1Mg,
    decimal VitaminB2Mg,
    decimal VitaminB3Mg,
    decimal VitaminB6Mg,
    decimal VitaminB12Mcg,
    decimal VitaminCMg,
    decimal VitaminDMcg,
    decimal VitaminEMg,
    decimal VitaminKMcg,
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
            list.Sum(entry => entry.Nutrition.SugarG),
            list.Sum(entry => entry.Nutrition.SodiumMg),
            list.Sum(entry => entry.Nutrition.CalciumMg),
            list.Sum(entry => entry.Nutrition.IronMg),
            list.Sum(entry => entry.Nutrition.MagnesiumMg),
            list.Sum(entry => entry.Nutrition.PotassiumMg),
            list.Sum(entry => entry.Nutrition.ZincMg),
            list.Sum(entry => entry.Nutrition.VitaminAMcg),
            list.Sum(entry => entry.Nutrition.VitaminB1Mg),
            list.Sum(entry => entry.Nutrition.VitaminB2Mg),
            list.Sum(entry => entry.Nutrition.VitaminB3Mg),
            list.Sum(entry => entry.Nutrition.VitaminB6Mg),
            list.Sum(entry => entry.Nutrition.VitaminB12Mcg),
            list.Sum(entry => entry.Nutrition.VitaminCMg),
            list.Sum(entry => entry.Nutrition.VitaminDMcg),
            list.Sum(entry => entry.Nutrition.VitaminEMg),
            list.Sum(entry => entry.Nutrition.VitaminKMcg),
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
    decimal FiberG,
    decimal SugarG,
    decimal SodiumMg,
    decimal CalciumMg,
    decimal IronMg,
    decimal MagnesiumMg,
    decimal PotassiumMg,
    decimal ZincMg,
    decimal VitaminAMcg,
    decimal VitaminB1Mg,
    decimal VitaminB2Mg,
    decimal VitaminB3Mg,
    decimal VitaminB6Mg,
    decimal VitaminB12Mcg,
    decimal VitaminCMg,
    decimal VitaminDMcg,
    decimal VitaminEMg,
    decimal VitaminKMcg,
    int EntryCount);