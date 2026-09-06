using System.Security.Claims;
using Calory.Api.Features;
using Calory.Api.Features.Reports;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calory.Api.Features.Reports.Trends;

public sealed class TrendsEndpoint(IUnitOfWork unitOfWork) : Endpoint<ReportQueryRequest, PagedResponse<TrendPoint>>
{
    public override void Configure()
    {
        Get("/api/reports/trends");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Summary(summary =>
        {
            summary.Summary = "Get nutrition trends";
            summary.Description = "Returns daily calorie and macro trend points for charts and comparisons.";
            summary.Response<PagedResponse<TrendPoint>>(200, "A page of daily trend points ordered chronologically.");
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
                return new TrendPoint(group.Key, totals.Calories, totals.ProteinG, totals.CarbohydratesG, totals.FatG,
                    totals.FiberG, totals.SugarG, totals.SodiumMg, totals.CalciumMg, totals.IronMg,
                    totals.MagnesiumMg, totals.PotassiumMg, totals.ZincMg, totals.VitaminAMcg,
                    totals.VitaminB1Mg, totals.VitaminB2Mg, totals.VitaminB3Mg, totals.VitaminB6Mg,
                    totals.VitaminB12Mcg, totals.VitaminCMg, totals.VitaminDMcg, totals.VitaminEMg,
                    totals.VitaminKMcg, totals.EntryCount);
            })
            .ToList();

        var (page, pageSize) = Pagination.Normalize(request.Page, request.PageSize);
        var paged = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        await Send.OkAsync(Pagination.Create(paged, page, pageSize, result.Count), cancellationToken);
    }
}