using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data;

public static class SeedDatabase
{
    public static void SeedCategories(ApplicationDbContext dbContext)
    {
        if (!dbContext.Categories.Any())
        {
            var categories = new List<Category>
            {
                Category.CreateCategory("Concerts"),
                Category.CreateCategory("Theater"),
                Category.CreateCategory("Sports"),
                Category.CreateCategory("Festivals"),
                Category.CreateCategory("Conferences"),
                Category.CreateCategory("Workshops"),
                Category.CreateCategory("Comedy Shows"),
                Category.CreateCategory("Family Events")
            };

            dbContext.Categories.AddRange(categories);
            dbContext.SaveChanges();

            Console.WriteLine("Seeded categories successfully.");
        }
        else
        {
            Console.WriteLine("Categories already exist.");
        }
    }
}