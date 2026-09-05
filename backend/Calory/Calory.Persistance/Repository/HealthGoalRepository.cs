using Calory.Domain;
using Calory.Persistance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Calory.Persistance.Repository;

public sealed class HealthGoalRepository(CaloryDbContext dbContext) : IHealthGoalRepository
{
    public async Task<IReadOnlyList<HealthGoal>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.HealthGoals
                            .AsNoTracking()
                            .Where(goal => goal.UserId == userId)
                            .OrderByDescending(goal => goal.StartDate)
                            .ThenByDescending(goal => goal.CreatedAt)
                            .ToListAsync(cancellationToken);
    }

    public Task<HealthGoal?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.HealthGoals.SingleOrDefaultAsync(goal => goal.Id == id && goal.UserId == userId, cancellationToken);
    }
        

    public Task<HealthGoal?> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.HealthGoals.SingleOrDefaultAsync(goal => goal.UserId == userId && goal.IsActive, cancellationToken);
    }


    public void Add(HealthGoal goal)
    {
        dbContext.HealthGoals.Add(goal);
    }
}