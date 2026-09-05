namespace Calory.Persistance.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IHealthGoalRepository HealthGoals { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}