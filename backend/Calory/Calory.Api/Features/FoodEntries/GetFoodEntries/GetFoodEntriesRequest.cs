namespace Calory.Api.Features.FoodEntries.GetFoodEntries;

using Calory.Domain.Enums;

public sealed class GetFoodEntriesRequest
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public MealType? MealType { get; set; }
    public decimal? MinCalories { get; set; }
    public decimal? MaxCalories { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}