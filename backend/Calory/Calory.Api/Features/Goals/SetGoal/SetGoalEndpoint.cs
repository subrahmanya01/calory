using System.Security.Claims;
using Calory.Domain;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.Goals.SetGoal;

public sealed class SetGoalEndpoint(IUnitOfWork unitOfWork) : Endpoint<SetGoalRequest, HealthGoalResponse>
{
    public override void Configure()
    {
        Post("/api/goals");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Set a health goal";
            summary.Description = "Creates a new health goal for the authenticated user and archives the previous active goal.";
            summary.Response<HealthGoalResponse>(201, "The health goal was created.");
            summary.Response(401, "The request does not contain a valid JWT.");
            summary.Response(400, "The goal values are invalid.");
            summary.Response(409, "The goal date range overlaps an existing goal.");
        });
    }

    public override async Task HandleAsync(SetGoalRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var existingGoals = await unitOfWork.HealthGoals.GetByUserIdAsync(userId, cancellationToken);
        if (existingGoals.Any(goal => GoalDateRange.Overlaps(goal, request.StartDate, request.EndDate)))
        {
            AddError("The goal date range overlaps an existing goal. Choose a period that does not overlap.");
            await Send.ErrorsAsync(409, cancellationToken);
            return;
        }

        var activeGoal = await unitOfWork.HealthGoals.GetActiveForUserAsync(userId, cancellationToken);
        if (activeGoal is not null)
            activeGoal.IsActive = false;

        var goal = new HealthGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DailyCalorieTarget = request.DailyCalorieTarget,
            ProteinTarget = request.ProteinTarget,
            CarbTarget = request.CarbTarget,
            FatTarget = request.FatTarget,
            WeightTarget = request.WeightTarget,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        unitOfWork.HealthGoals.Add(goal);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await Send.ResponseAsync(HealthGoalResponse.From(goal), 201, cancellationToken);
    }
}