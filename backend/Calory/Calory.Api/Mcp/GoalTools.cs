using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using Calory.Api.Features.Goals;
using Calory.Api.Features.Goals.SetGoal;
using Calory.Api.Features.Goals.UpdateGoal;
using Calory.Domain;
using Calory.Persistance.Interfaces;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Calory.Api.Mcp;

[McpServerToolType]
public sealed class GoalTools(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
{
    [McpServerTool, Description("Gets all health goals for the authenticated user.")]
    public async Task<string> GetHealthGoals(CancellationToken cancellationToken = default)
    {
        var goals = await unitOfWork.HealthGoals.GetByUserIdAsync(GetUserId(), cancellationToken);
        return JsonSerializer.Serialize(goals.Select(HealthGoalResponse.From));
    }

    [McpServerTool, Description("Gets the authenticated user's active health goal and calorie and macro targets.")]
    public async Task<string> GetActiveHealthGoal(CancellationToken cancellationToken = default)
    {
        var goal = await unitOfWork.HealthGoals.GetActiveForUserAsync(GetUserId(), cancellationToken);
        return JsonSerializer.Serialize(goal is null ? null : HealthGoalResponse.From(goal));
    }

    [McpServerTool, Description("Creates a new active health goal and archives the previous active goal.")]
    public async Task<string> SetHealthGoal(
        [Description("Goal targets and dates. EndDate is optional and dates use YYYY-MM-DD format.")] SetGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var userId = GetUserId();
        var activeGoal = await unitOfWork.HealthGoals.GetActiveForUserAsync(userId, cancellationToken);
        if (activeGoal is not null)
            activeGoal.IsActive = false;

        var goal = new HealthGoal
        {
            Id = Guid.NewGuid(), UserId = userId,
            DailyCalorieTarget = request.DailyCalorieTarget,
            ProteinTarget = request.ProteinTarget, CarbTarget = request.CarbTarget,
            FatTarget = request.FatTarget, WeightTarget = request.WeightTarget,
            StartDate = request.StartDate, EndDate = request.EndDate,
            IsActive = true, CreatedAt = DateTime.UtcNow
        };

        unitOfWork.HealthGoals.Add(goal);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return JsonSerializer.Serialize(HealthGoalResponse.From(goal));
    }

    [McpServerTool, Description("Updates a health goal owned by the authenticated user.")]
    public async Task<string> UpdateHealthGoal(
        [Description("Goal id and replacement targets and dates.")] UpdateGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var goal = await unitOfWork.HealthGoals.GetByIdForUserAsync(request.Id, GetUserId(), cancellationToken)
            ?? throw new KeyNotFoundException("Health goal was not found.");

        goal.DailyCalorieTarget = request.DailyCalorieTarget;
        goal.ProteinTarget = request.ProteinTarget;
        goal.CarbTarget = request.CarbTarget;
        goal.FatTarget = request.FatTarget;
        goal.WeightTarget = request.WeightTarget;
        goal.StartDate = request.StartDate;
        goal.EndDate = request.EndDate;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return JsonSerializer.Serialize(HealthGoalResponse.From(goal));
    }

    private Guid GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("A valid authenticated user is required.");
    }

    private static void Validate(GoalRequest request)
    {
        if (request.DailyCalorieTarget <= 0 || request.DailyCalorieTarget > 10000 ||
            request.ProteinTarget < 0 || request.ProteinTarget > 1000 ||
            request.CarbTarget < 0 || request.CarbTarget > 2000 ||
            request.FatTarget < 0 || request.FatTarget > 1000 ||
            request.WeightTarget <= 0 || request.WeightTarget > 1000)
            throw new ArgumentException("Goal targets are outside the supported ranges.");
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
            throw new ArgumentException("EndDate cannot precede StartDate.");
    }
}