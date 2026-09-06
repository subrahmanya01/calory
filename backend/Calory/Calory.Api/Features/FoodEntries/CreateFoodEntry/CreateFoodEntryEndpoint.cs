using System.Security.Claims;
using Calory.Api.Features.FoodEntries;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.FoodEntries.CreateFoodEntry;

public sealed class CreateFoodEntryEndpoint(IUnitOfWork unitOfWork) : Endpoint<FoodEntryRequest, FoodEntryResponse>
{
    public override void Configure()
    {
        Post("/api/food-entries");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Add a food entry";
            summary.Description = "Adds a food entry and its nutrition data for the authenticated user.";
            summary.Response<FoodEntryResponse>(201, "The food entry was created.");
            summary.Response(401, "A valid JWT is required.");
        });
    }

    public override async Task HandleAsync(FoodEntryRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var entry = request.ToEntity(userId);
        unitOfWork.FoodEntries.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await Send.ResponseAsync(FoodEntryResponse.From(entry), 201, cancellationToken);
    }
}