using Calory.Persistance.Interfaces;

namespace Calory.Persistance;

public sealed class UnitOfWork(
    CaloryDbContext dbContext,
    IUserRepository users,
    IHealthGoalRepository healthGoals,
    IFoodEntryRepository foodEntries) : IUnitOfWork
{
    public IUserRepository Users { get; } = users;
    public IHealthGoalRepository HealthGoals { get; } = healthGoals;
    public IFoodEntryRepository FoodEntries { get; } = foodEntries;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}