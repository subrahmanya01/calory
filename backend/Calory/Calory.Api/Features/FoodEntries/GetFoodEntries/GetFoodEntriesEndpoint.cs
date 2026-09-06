using System.Security.Claims;
using Calory.Api.Features.FoodEntries;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.FoodEntries.GetFoodEntries;

public sealed class GetFoodEntriesEndpoint(IUnitOfWork unitOfWork) : Endpoint<GetFoodEntriesRequest, List<FoodEntryResponse>>
{
    public override void Configure()
    {
        Get("/api/food-entries");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Get food entries";
            summary.Description = "Returns the authenticated user's food entries. Use from and to as inclusive calendar dates.";
            summary.Response<List<FoodEntryResponse>>(200, "Food entries ordered newest first.");
            summary.Response(401, "A valid JWT is required.");
        });
    }

    public override async Task HandleAsync(GetFoodEntriesRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var from = request.From?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? DateTime.UtcNow.Date.AddDays(-30);
        var to = request.To?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1) ?? DateTime.UtcNow.Date.AddDays(1);
        if (to <= from)
        {
            AddError("The 'to' date must be on or after the 'from' date.");
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        var entries = await unitOfWork.FoodEntries.GetByUserAndRangeAsync(userId, from, to, cancellationToken);
        await Send.OkAsync(entries.Select(FoodEntryResponse.From).ToList(), cancellationToken);
    }
}