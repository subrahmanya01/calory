using FastEndpoints;
using FluentValidation;

namespace Calory.Api.Features.FoodEntries;

public sealed class FoodEntryRequestValidator : Validator<FoodEntryRequest>
{
    public FoodEntryRequestValidator()
    {
        RuleFor(request => request.FoodName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Quantity).GreaterThan(0);
        RuleFor(request => request.Unit).NotEmpty().MaximumLength(40);
        RuleFor(request => request.ConsumedAt).NotEmpty();
        RuleFor(request => request.Nutrition).NotNull();
        RuleFor(request => request.Nutrition.Calories).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Nutrition.ProteinG).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Nutrition.CarbohydratesG).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Nutrition.FatG).GreaterThanOrEqualTo(0);
    }
}