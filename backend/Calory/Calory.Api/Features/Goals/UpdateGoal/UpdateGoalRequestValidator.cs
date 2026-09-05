using Calory.Api.Features.Goals;
using FluentValidation;

namespace Calory.Api.Features.Goals.UpdateGoal;

public sealed class UpdateGoalRequestValidator : GoalRequestValidator<UpdateGoalRequest>
{
    public UpdateGoalRequestValidator()
    {
        RuleFor(request => request.Id).NotEqual(Guid.Empty);
    }
}