using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();

    public Category(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public static Category CreateCategory(string name)
    {
        return new Category(name);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Category name cannot be empty.");
        if (newName.Length > 100)
            throw new ArgumentException("Category name cannot exceed 100 characters.");

        Name = newName;
    }
}