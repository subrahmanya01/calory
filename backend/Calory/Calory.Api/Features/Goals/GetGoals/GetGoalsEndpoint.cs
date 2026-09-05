using System.Security.Claims;
using Calory.Persistance.Repository;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.Goals.GetGoals;

public sealed class GetGoalsEndpoint(IUnitOfWork unitOfWork) : EndpointWithoutRequest<List<HealthGoalResponse>>
{
    public override void Configure()
    {
        Get("/api/goals");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Get the authenticated user's health goals";
            summary.Description = "Returns all health goals owned by the authenticated user, newest first.";
            summary.Response<List<HealthGoalResponse>>(200, "The user's health goals.");
            summary.Response(401, "The request does not contain a valid JWT.");
        });
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var goals = await unitOfWork.HealthGoals.GetByUserIdAsync(userId, cancellationToken);
        await Send.OkAsync(goals.Select(HealthGoalResponse.From).ToList(), cancellationToken);
    }
}