using System.Security.Claims;
using Calory.Api.Features.FoodEntries;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.FoodEntries.UpdateFoodEntry;

public sealed class UpdateFoodEntryEndpoint(IUnitOfWork unitOfWork) : Endpoint<UpdateFoodEntryRequest, FoodEntryResponse>
{
    public override void Configure()
    {
        Put("/api/food-entries/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Edit a food entry";
            summary.Description = "Updates a food entry owned by the authenticated user.";
            summary.Response<FoodEntryResponse>(200, "The food entry was updated.");
            summary.Response(404, "The food entry was not found for this user.");
        });
    }

    public override async Task HandleAsync(UpdateFoodEntryRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var entry = await unitOfWork.FoodEntries.GetByIdForUserAsync(request.Id, userId, cancellationToken);
        if (entry is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        request.ToEntity(userId, entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await Send.OkAsync(FoodEntryResponse.From(entry), cancellationToken);
    }
}