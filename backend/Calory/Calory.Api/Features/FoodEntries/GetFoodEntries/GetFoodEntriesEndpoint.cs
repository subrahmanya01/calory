using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Calory.Api.Features.FoodEntries.GetFoodEntries;

public sealed class GetFoodEntriesEndpoint(IUnitOfWork unitOfWork) : Endpoint<GetFoodEntriesRequest, PagedResponse<FoodEntryResponse>>
{
    public override void Configure()
    {
        Get("/api/food-entries");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Get food entries";
            summary.Description = "Returns the authenticated user's food entries. Use from and to as inclusive calendar dates.";
            summary.Response<PagedResponse<FoodEntryResponse>>(200, "A page of food entries ordered newest first.");
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
        if (to <= from || request.MinCalories < 0 || request.MaxCalories < 0 ||
            request.MinCalories.HasValue && request.MaxCalories.HasValue && request.MaxCalories < request.MinCalories)
        {
            AddError("The 'to' date must be on or after the 'from' date.");
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        var (page, pageSize) = Pagination.Normalize(request.Page, request.PageSize);
        var entries = await unitOfWork.FoodEntries.GetByUserAndRangeAsync(userId, from, to, cancellationToken);
        var filtered = entries
            .Where(entry => !request.MealType.HasValue || entry.MealType == request.MealType)
            .Where(entry => !request.MinCalories.HasValue || entry.Nutrition.Calories >= request.MinCalories)
            .Where(entry => !request.MaxCalories.HasValue || entry.Nutrition.Calories <= request.MaxCalories)
            .ToList();
        var result = filtered.Skip((page - 1) * pageSize).Take(pageSize).Select(FoodEntryResponse.From).ToList();
        await Send.OkAsync(Pagination.Create(result, page, pageSize, filtered.Count), cancellationToken);
    }
}