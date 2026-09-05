using Calory.Domain;

namespace Calory.Persistance.Interfaces;

public interface IHealthGoalRepository
{
    Task<IReadOnlyList<HealthGoal>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<HealthGoal?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<HealthGoal?> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(HealthGoal goal);
}