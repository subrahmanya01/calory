using Calory.Api.Features.FoodEntries;
using Calory.Persistance.Interfaces;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;

namespace Calory.Api.Mcp;

[McpServerToolType]
public sealed class FoodEntryTools(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
{
    [McpServerTool, Description("Adds a food entry with its nutrition data for the authenticated user.")]
    public async Task<string> AddFoodEntry(
        [Description("Food entry details, including meal type, food name, quantity, consumedAt, and nutrition values.")] FoodEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var entry = request.ToEntity(GetUserId());
        unitOfWork.FoodEntries.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return JsonSerializer.Serialize(FoodEntryResponse.From(entry));
    }

    [McpServerTool, Description("Updates a food entry owned by the authenticated user.")]
    public async Task<string> UpdateFoodEntry(
        [Description("Food entry id and replacement food and nutrition details.")] UpdateFoodEntryToolRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var userId = GetUserId();
        var entry = await unitOfWork.FoodEntries.GetByIdForUserAsync(request.Id, userId, cancellationToken)
            ?? throw new KeyNotFoundException("Food entry was not found.");

        request.ToFoodEntryRequest().ToEntity(userId, entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return JsonSerializer.Serialize(FoodEntryResponse.From(entry));
    }

    [McpServerTool, Description("Deletes a food entry owned by the authenticated user.")]
    public async Task<string> DeleteFoodEntry(
        [Description("The food entry id to delete.")] Guid id,
        CancellationToken cancellationToken = default)
    {
        var entry = await unitOfWork.FoodEntries.GetByIdForUserAsync(id, GetUserId(), cancellationToken)
            ?? throw new KeyNotFoundException("Food entry was not found.");

        unitOfWork.FoodEntries.Remove(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return JsonSerializer.Serialize(new { Deleted = true, Id = id });
    }

    private Guid GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("A valid authenticated user is required.");
    }

    private static void Validate(FoodEntryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FoodName) || request.FoodName.Length > 200)
            throw new ArgumentException("FoodName is required and must be 200 characters or fewer.");
        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Unit) || request.Unit.Length > 40)
            throw new ArgumentException("Unit is required and must be 40 characters or fewer.");
        if (request.ConsumedAt == default)
            throw new ArgumentException("ConsumedAt is required.");
        if (request.Nutrition is null)
            throw new ArgumentException("Nutrition is required.");
        if (request.Nutrition.Calories < 0 || request.Nutrition.ProteinG < 0 ||
            request.Nutrition.CarbohydratesG < 0 || request.Nutrition.FatG < 0)
            throw new ArgumentException("Calories and macronutrients cannot be negative.");
    }
}

public sealed class UpdateFoodEntryToolRequest : FoodEntryRequest
{
    public Guid Id { get; set; }

    public FoodEntryRequest ToFoodEntryRequest() => new()
    {
        MealType = MealType,
        FoodName = FoodName,
        Quantity = Quantity,
        Unit = Unit,
        ConsumedAt = ConsumedAt,
        Source = Source,
        Notes = Notes,
        Nutrition = Nutrition
    };
}