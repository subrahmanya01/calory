using Calory.Domain;

namespace Calory.Persistance.Interfaces;

public interface IFoodEntryRepository
{
    Task<IReadOnlyList<FoodEntry>> GetByUserAndRangeAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<FoodEntry?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    void Add(FoodEntry entry);
    void Remove(FoodEntry entry);
}