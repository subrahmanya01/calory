using System.Security.Claims;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.FoodEntries.DeleteFoodEntry;

public sealed class DeleteFoodEntryEndpoint(IUnitOfWork unitOfWork) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/food-entries/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Delete a food entry";
            summary.Description = "Deletes a food entry owned by the authenticated user.";
            summary.Response(204, "The food entry was deleted.");
            summary.Response(404, "The food entry was not found for this user.");
        });
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var entry = await unitOfWork.FoodEntries.GetByIdForUserAsync(Route<Guid>("id"), userId, cancellationToken);
        if (entry is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        unitOfWork.FoodEntries.Remove(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await Send.NoContentAsync(cancellationToken);
    }
}