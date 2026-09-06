using Calory.Domain;
using Calory.Persistance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Calory.Persistance.Repository;

public sealed class FoodEntryRepository(CaloryDbContext dbContext) : IFoodEntryRepository
{
    public async Task<IReadOnlyList<FoodEntry>> GetByUserAndRangeAsync(
        Guid userId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default) =>
        await dbContext.FoodEntries
            .AsNoTracking()
            .Include(entry => entry.Nutrition)
            .Where(entry => entry.UserId == userId && entry.ConsumedAt >= from && entry.ConsumedAt < to)
            .OrderByDescending(entry => entry.ConsumedAt)
            .ToListAsync(cancellationToken);

    public Task<FoodEntry?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.FoodEntries
            .Include(entry => entry.Nutrition)
            .SingleOrDefaultAsync(entry => entry.Id == id && entry.UserId == userId, cancellationToken);

    public void Add(FoodEntry entry) => dbContext.FoodEntries.Add(entry);

    public void Remove(FoodEntry entry) => dbContext.FoodEntries.Remove(entry);
}