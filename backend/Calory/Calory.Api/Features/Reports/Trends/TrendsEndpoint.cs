using System.Security.Claims;
using Calory.Api.Features.Reports;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.Reports.Trends;

public sealed class TrendsEndpoint(IUnitOfWork unitOfWork) : Endpoint<ReportQueryRequest, List<TrendPoint>>
{
    public override void Configure()
    {
        Get("/api/reports/trends");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Get nutrition trends";
            summary.Description = "Returns daily calorie and macro trend points for charts and comparisons.";
            summary.Response<List<TrendPoint>>(200, "Daily trend points ordered chronologically.");
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
            .Select(group =>
            {
                var totals = NutritionTotals.From(group);
                return new TrendPoint(group.Key, totals.Calories, totals.ProteinG, totals.CarbohydratesG, totals.FatG, totals.EntryCount);
            })
            .ToList();

        await Send.OkAsync(result, cancellationToken);
    }
}