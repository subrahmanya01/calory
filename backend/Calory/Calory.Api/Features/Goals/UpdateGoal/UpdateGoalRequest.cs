using Calory.Api.Features.Goals;

namespace Calory.Api.Features.Goals.UpdateGoal;

public sealed class UpdateGoalRequest : GoalRequest
{
    public Guid Id { get; set; }
}