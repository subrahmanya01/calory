using Calory.Domain;
using Calory.Domain.Enums;
using Calory.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Calory.Api.Infrastructure;

/// <summary>
/// Only feeds data for development purposes. This class should not be used in production environments.
/// </summary>
public static class DevelopmentDataSeeder
{
    private const string SeedMarker = "Development demo seed 2026-09";

    private static readonly (string Name, MealType Meal, decimal Calories, decimal Protein, decimal Carbs, decimal Fat, decimal Fiber)[] Foods =
    [
        ("Overnight oats with berries", MealType.Breakfast, 420, 18, 62, 12, 9),
        ("Greek yogurt and granola", MealType.Breakfast, 360, 24, 45, 10, 5),
        ("Egg and avocado toast", MealType.Breakfast, 510, 23, 42, 27, 8),
        ("Banana peanut butter toast", MealType.Breakfast, 390, 14, 53, 16, 7),
        ("Vegetable omelette", MealType.Breakfast, 330, 25, 14, 20, 4),
        ("Chicken quinoa bowl", MealType.Lunch, 640, 48, 66, 19, 11),
        ("Lentil and roasted vegetable bowl", MealType.Lunch, 570, 27, 78, 16, 18),
        ("Turkey avocado wrap", MealType.Lunch, 610, 39, 55, 25, 8),
        ("Tuna rice salad", MealType.Lunch, 530, 42, 58, 12, 6),
        ("Paneer tikka with rice", MealType.Lunch, 680, 31, 72, 28, 7),
        ("Salmon with sweet potato", MealType.Dinner, 720, 46, 58, 31, 9),
        ("Tofu stir-fry with noodles", MealType.Dinner, 620, 29, 76, 22, 10),
        ("Chicken pasta primavera", MealType.Dinner, 760, 45, 88, 24, 8),
        ("Bean burrito plate", MealType.Dinner, 690, 32, 91, 21, 16),
        ("Vegetable curry with naan", MealType.Dinner, 710, 21, 94, 27, 13),
        ("Apple and almonds", MealType.Snack, 220, 6, 27, 11, 6),
        ("Cottage cheese and fruit", MealType.Snack, 190, 20, 18, 4, 3),
        ("Hummus and carrots", MealType.Snack, 240, 8, 27, 12, 8),
        ("Protein smoothie", MealType.Snack, 280, 26, 31, 7, 5),
        ("Dark chocolate and walnuts", MealType.Snack, 210, 4, 18, 14, 3)
    ];

    public static async Task SeedAsync(CaloryDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .Where(user => user.IsActive)
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
            return;

        var userIds = users.Select(user => user.Id).ToList();
        var alreadySeeded = await dbContext.FoodEntries
            .AnyAsync(entry => userIds.Contains(entry.UserId) && entry.Notes == SeedMarker, cancellationToken);

        if (alreadySeeded)
            return;

        var now = DateTime.UtcNow;
        var entries = new List<FoodEntry>(users.Count * 50);

        foreach (var user in users)
        {
            for (var index = 0; index < 50; index++)
            {
                var food = Foods[index % Foods.Length];
                var dayOffset = index % 14;
                var mealNumber = index / 14;
                var consumedAt = now.Date.AddDays(-dayOffset).AddHours(7 + (mealNumber * 3) + (index % 2));
                var variation = (index % 5 - 2) * 12;
                var entryId = Guid.NewGuid();

                entries.Add(new FoodEntry
                {
                    Id = entryId,
                    UserId = user.Id,
                    MealType = food.Meal,
                    FoodName = food.Name,
                    Quantity = 1,
                    Unit = "serving",
                    ConsumedAt = consumedAt,
                    Source = FoodEntrySource.Database,
                    Notes = SeedMarker,
                    CreatedAt = consumedAt,
                    UpdatedAt = consumedAt,
                    Nutrition = new FoodNutrition
                    {
                        Id = Guid.NewGuid(),
                        FoodEntryId = entryId,
                        Calories = Math.Max(0, food.Calories + variation),
                        ProteinG = food.Protein,
                        CarbohydratesG = food.Carbs,
                        FatG = food.Fat,
                        FiberG = food.Fiber,
                        SugarG = food.Carbs / 5,
                        SodiumMg = 250 + (index * 17 % 500),
                        CalciumMg = 80 + (index * 11 % 180),
                        IronMg = 1 + (index % 6),
                        MagnesiumMg = 20 + (index * 7 % 80),
                        PotassiumMg = 180 + (index * 29 % 420)
                    }
                });
            }
        }

        dbContext.FoodEntries.AddRange(entries);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
