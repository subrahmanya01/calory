using Calory.Domain;

namespace Calory.Api.Features.FoodEntries;

public static class FoodEntryMapper
{
    public static FoodEntry ToEntity(this FoodEntryRequest request, Guid userId, FoodEntry? existing = null)
    {
        var entry = existing ?? new FoodEntry { Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow };
        entry.MealType = request.MealType;
        entry.FoodName = request.FoodName.Trim();
        entry.Quantity = request.Quantity;
        entry.Unit = request.Unit.Trim();
        entry.ConsumedAt = request.ConsumedAt.ToUniversalTime();
        entry.Source = request.Source;
        entry.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entry.UpdatedAt = DateTime.UtcNow;
        entry.Nutrition ??= new FoodNutrition { Id = Guid.NewGuid(), FoodEntryId = entry.Id };
        entry.Nutrition.Calories = request.Nutrition.Calories;
        entry.Nutrition.ProteinG = request.Nutrition.ProteinG;
        entry.Nutrition.CarbohydratesG = request.Nutrition.CarbohydratesG;
        entry.Nutrition.FatG = request.Nutrition.FatG;
        entry.Nutrition.FiberG = request.Nutrition.FiberG;
        entry.Nutrition.SugarG = request.Nutrition.SugarG;
        entry.Nutrition.SodiumMg = request.Nutrition.SodiumMg;
        entry.Nutrition.CalciumMg = request.Nutrition.CalciumMg;
        entry.Nutrition.IronMg = request.Nutrition.IronMg;
        entry.Nutrition.MagnesiumMg = request.Nutrition.MagnesiumMg;
        entry.Nutrition.PotassiumMg = request.Nutrition.PotassiumMg;
        entry.Nutrition.ZincMg = request.Nutrition.ZincMg;
        entry.Nutrition.VitaminAMcg = request.Nutrition.VitaminAMcg;
        entry.Nutrition.VitaminB1Mg = request.Nutrition.VitaminB1Mg;
        entry.Nutrition.VitaminB2Mg = request.Nutrition.VitaminB2Mg;
        entry.Nutrition.VitaminB3Mg = request.Nutrition.VitaminB3Mg;
        entry.Nutrition.VitaminB6Mg = request.Nutrition.VitaminB6Mg;
        entry.Nutrition.VitaminB12Mcg = request.Nutrition.VitaminB12Mcg;
        entry.Nutrition.VitaminCMg = request.Nutrition.VitaminCMg;
        entry.Nutrition.VitaminDMcg = request.Nutrition.VitaminDMcg;
        entry.Nutrition.VitaminEMg = request.Nutrition.VitaminEMg;
        entry.Nutrition.VitaminKMcg = request.Nutrition.VitaminKMcg;
        return entry;
    }
}