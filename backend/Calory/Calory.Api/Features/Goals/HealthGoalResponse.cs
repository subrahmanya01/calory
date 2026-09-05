using Calory.Domain;

namespace Calory.Api.Features.Goals;

public sealed record HealthGoalResponse(
    Guid Id,
    decimal DailyCalorieTarget,
    decimal ProteinTarget,
    decimal CarbTarget,
    decimal FatTarget,
    decimal WeightTarget,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateTime CreatedAt)
{
    public static HealthGoalResponse From(HealthGoal goal) => new(
        goal.Id,
        goal.DailyCalorieTarget,
        goal.ProteinTarget,
        goal.CarbTarget,
        goal.FatTarget,
        goal.WeightTarget,
        goal.StartDate,
        goal.EndDate,
        goal.IsActive,
        goal.CreatedAt);
}