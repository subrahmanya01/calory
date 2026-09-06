namespace Calory.Api.Features.FoodEntries.GetFoodEntries;

public sealed class GetFoodEntriesRequest
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}