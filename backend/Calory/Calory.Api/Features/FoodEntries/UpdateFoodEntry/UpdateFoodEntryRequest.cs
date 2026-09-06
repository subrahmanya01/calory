using Calory.Api.Features.FoodEntries;

namespace Calory.Api.Features.FoodEntries.UpdateFoodEntry;

public sealed class UpdateFoodEntryRequest : FoodEntryRequest
{
    public Guid Id { get; set; }
}