using System.Security.Claims;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.Goals.UpdateGoal;

public sealed class UpdateGoalEndpoint(IUnitOfWork unitOfWork) : Endpoint<UpdateGoalRequest, HealthGoalResponse>
{
    public override void Configure()
    {
        Put("/api/goals/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Edit a health goal";
            summary.Description = "Updates a health goal owned by the authenticated user.";
            summary.Response<HealthGoalResponse>(200, "The health goal was updated.");
            summary.Response(401, "The request does not contain a valid JWT.");
            summary.Response(404, "The goal was not found for the authenticated user.");
        });
    }

    public override async Task HandleAsync(UpdateGoalRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var goal = await unitOfWork.HealthGoals.GetByIdForUserAsync(request.Id, userId, cancellationToken);
        if (goal is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        goal.DailyCalorieTarget = request.DailyCalorieTarget;
        goal.ProteinTarget = request.ProteinTarget;
        goal.CarbTarget = request.CarbTarget;
        goal.FatTarget = request.FatTarget;
        goal.WeightTarget = request.WeightTarget;
        goal.StartDate = request.StartDate;
        goal.EndDate = request.EndDate;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await Send.OkAsync(HealthGoalResponse.From(goal), cancellationToken);
    }
}