using System.Security.Claims;
using Calory.Api.Features;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.Goals.GetGoals;

public sealed class GetGoalsEndpoint(IUnitOfWork unitOfWork) : Endpoint<GetGoalsRequest, PagedResponse<HealthGoalResponse>>
{
    public override void Configure()
    {
        Get("/api/goals");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Get the authenticated user's health goals";
            summary.Description = "Returns all health goals owned by the authenticated user, newest first.";
            summary.Response<PagedResponse<HealthGoalResponse>>(200, "A page of the user's health goals.");
            summary.Response(401, "The request does not contain a valid JWT.");
        });
    }

    public override async Task HandleAsync(GetGoalsRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var (page, pageSize) = Pagination.Normalize(request.Page, request.PageSize);
        var goals = await unitOfWork.HealthGoals.GetByUserIdAsync(userId, cancellationToken);
        var result = goals.Skip((page - 1) * pageSize).Take(pageSize).Select(HealthGoalResponse.From).ToList();
        await Send.OkAsync(Pagination.Create(result, page, pageSize, goals.Count), cancellationToken);
    }
}