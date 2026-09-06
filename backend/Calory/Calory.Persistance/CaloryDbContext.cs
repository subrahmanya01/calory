using Calory.Domain;
using Microsoft.EntityFrameworkCore;

namespace Calory.Persistance;

public sealed class CaloryDbContext(DbContextOptions<CaloryDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<HealthGoal> HealthGoals => Set<HealthGoal>();
    public DbSet<FoodEntry> FoodEntries => Set<FoodEntry>();
    public DbSet<FoodNutrition> FoodNutrition => Set<FoodNutrition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<HealthGoal>(entity =>
        {
            entity.HasKey(goal => goal.Id);
            entity.HasIndex(goal => new { goal.UserId, goal.IsActive });
            entity.Property(goal => goal.DailyCalorieTarget).HasPrecision(10, 2).IsRequired();
            entity.Property(goal => goal.ProteinTarget).HasPrecision(10, 2).IsRequired();
            entity.Property(goal => goal.CarbTarget).HasPrecision(10, 2).IsRequired();
            entity.Property(goal => goal.FatTarget).HasPrecision(10, 2).IsRequired();
            entity.Property(goal => goal.WeightTarget).HasPrecision(10, 2).IsRequired();
        });

        modelBuilder.Entity<FoodEntry>(entity =>
        {
            entity.HasKey(entry => entry.Id);
            entity.HasIndex(entry => new { entry.UserId, entry.ConsumedAt });
            entity.Property(entry => entry.MealType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(entry => entry.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(entry => entry.FoodName).HasMaxLength(200).IsRequired();
            entity.Property(entry => entry.Unit).HasMaxLength(40).IsRequired();
            entity.HasOne(entry => entry.Nutrition)
                .WithOne()
                .HasForeignKey<FoodNutrition>(nutrition => nutrition.FoodEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FoodNutrition>(entity =>
        {
            entity.HasKey(nutrition => nutrition.Id);
            entity.Property(nutrition => nutrition.Calories).HasPrecision(10, 2).IsRequired();
            entity.Property(nutrition => nutrition.ProteinG).HasPrecision(10, 2).IsRequired();
            entity.Property(nutrition => nutrition.CarbohydratesG).HasPrecision(10, 2).IsRequired();
            entity.Property(nutrition => nutrition.FatG).HasPrecision(10, 2).IsRequired();
        });
    }
}