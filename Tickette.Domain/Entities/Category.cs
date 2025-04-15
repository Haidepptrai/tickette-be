using Tickette.Domain.Common;
using MissingFieldException = Tickette.Domain.Common.Exceptions.InvalidFieldException;

namespace Tickette.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();

    public Category(string name)
    {
        Name = name;
    }

    public static Category CreateCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new MissingFieldException("Category name.");

        return new Category(name);
    }
}