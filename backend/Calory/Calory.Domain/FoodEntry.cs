using Calory.Domain.Enums;

namespace Calory.Domain;

public sealed class FoodEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public MealType MealType { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime ConsumedAt { get; set; }
    public FoodEntrySource Source { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public FoodNutrition Nutrition { get; set; } = new();
}