namespace Calory.Api.Features.Goals.GetGoals;

public sealed class GetGoalsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}