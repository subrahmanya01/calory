namespace Calory.Persistance.Repository;

public sealed class UnitOfWork(CaloryDbContext dbContext, IUserRepository users) : IUnitOfWork
{
    public IUserRepository Users { get; } = users;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}