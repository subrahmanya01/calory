using FastEndpoints;
using FluentValidation;

namespace Calory.Api.Features.Goals;

public abstract class GoalRequestValidator<TRequest> : Validator<TRequest>
    where TRequest : GoalRequest
{
    protected GoalRequestValidator()
    {
        RuleFor(request => request.DailyCalorieTarget).GreaterThan(0).LessThanOrEqualTo(10000);
        RuleFor(request => request.ProteinTarget).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000);
        RuleFor(request => request.CarbTarget).GreaterThanOrEqualTo(0).LessThanOrEqualTo(2000);
        RuleFor(request => request.FatTarget).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000);
        RuleFor(request => request.WeightTarget).GreaterThan(0).LessThanOrEqualTo(1000);
        RuleFor(request => request.EndDate)
            .GreaterThanOrEqualTo(request => request.StartDate)
            .When(request => request.EndDate.HasValue);
    }
}

public abstract class GoalRequest
{
    public decimal DailyCalorieTarget { get; set; }
    public decimal ProteinTarget { get; set; }
    public decimal CarbTarget { get; set; }
    public decimal FatTarget { get; set; }
    public decimal WeightTarget { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}