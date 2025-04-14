using Tickette.Domain.Common;
using Tickette.Domain.Common.Exceptions;
using MissingFieldException = Tickette.Domain.Common.Exceptions.InvalidFieldException;

namespace Tickette.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();

    public Category(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public static Category CreateCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new MissingFieldException("Category name.");

        return new Category(name);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new MissingFieldException("Category name cannot be empty.");
        if (newName.Length > 100)
            throw new ValidateFieldException("Category name", "Category name cannot exceed 100 characters.");

        Name = newName;
    }
}