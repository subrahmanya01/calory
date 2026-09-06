using Calory.Domain;

namespace Calory.Api.Features.FoodEntries;

public sealed record FoodNutritionResponse(
    decimal Calories,
    decimal ProteinG,
    decimal CarbohydratesG,
    decimal FatG,
    decimal FiberG,
    decimal SugarG,
    decimal SodiumMg)
{
    public static FoodNutritionResponse From(FoodNutrition nutrition) => new(
        nutrition.Calories,
        nutrition.ProteinG,
        nutrition.CarbohydratesG,
        nutrition.FatG,
        nutrition.FiberG,
        nutrition.SugarG,
        nutrition.SodiumMg);
}

public sealed record FoodEntryResponse(
    Guid Id,
    MealType MealType,
    string FoodName,
    decimal Quantity,
    string Unit,
    DateTime ConsumedAt,
    FoodEntrySource Source,
    string? Notes,
    FoodNutritionResponse Nutrition)
{
    public static FoodEntryResponse From(FoodEntry entry) => new(
        entry.Id,
        entry.MealType,
        entry.FoodName,
        entry.Quantity,
        entry.Unit,
        entry.ConsumedAt,
        entry.Source,
        entry.Notes,
        FoodNutritionResponse.From(entry.Nutrition));
}