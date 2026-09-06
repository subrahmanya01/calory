namespace Calory.Persistance.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IHealthGoalRepository HealthGoals { get; }
    IFoodEntryRepository FoodEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}