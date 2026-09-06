using Calory.Domain;

namespace Calory.Api.Features.Goals;

public static class GoalDateRange
{
    public static bool Overlaps(HealthGoal goal, DateOnly startDate, DateOnly? endDate, Guid? excludedGoalId = null)
    {
        if (excludedGoalId.HasValue && goal.Id == excludedGoalId.Value)
            return false;

        var newEnd = endDate ?? DateOnly.MaxValue;
        var existingEnd = goal.EndDate ?? DateOnly.MaxValue;
        return goal.StartDate <= newEnd && existingEnd >= startDate;
    }

    public static bool IncludesToday(HealthGoal goal)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return goal.StartDate <= today && (!goal.EndDate.HasValue || goal.EndDate.Value >= today);
    }
}
