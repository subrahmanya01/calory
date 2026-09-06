using System.Security.Claims;
using Calory.Api.Features;
using Calory.Api.Features.Reports;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.Reports.DailySummary;

public sealed class DailySummaryEndpoint(IUnitOfWork unitOfWork) : Endpoint<ReportQueryRequest, PagedResponse<DailyNutritionResponse>>
{
    public override void Configure()
    {
        Get("/api/reports/daily");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Get daily nutrition totals";
            summary.Description = "Returns calories and macronutrient totals grouped by calendar day for the authenticated user.";
            summary.Response<PagedResponse<DailyNutritionResponse>>(200, "A page of daily nutrition totals.");
            summary.Response(400, "The date range is invalid.");
        });
    }

    public override async Task HandleAsync(ReportQueryRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var range = request.ToUtcRange();
        if (range is null)
        {
            await Send.StatusCodeAsync(400, cancellationToken);
            return;
        }

        var entries = await unitOfWork.FoodEntries.GetByUserAndRangeAsync(userId, range.Value.From, range.Value.To, cancellationToken);
        var result = entries
            .GroupBy(entry => DateOnly.FromDateTime(entry.ConsumedAt.ToUniversalTime()))
            .OrderBy(group => group.Key)
            .Select(group => new DailyNutritionResponse(group.Key, NutritionTotals.From(group)))
            .ToList();

        var (page, pageSize) = Pagination.Normalize(request.Page, request.PageSize);
        var paged = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        await Send.OkAsync(Pagination.Create(paged, page, pageSize, result.Count), cancellationToken);
    }
}