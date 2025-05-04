using Tickette.Domain.Common;
using MissingFieldException = Tickette.Domain.Common.Exceptions.InvalidFieldException;

namespace Tickette.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();

    private Category(string name)
    {
        Name = name;
    }

    private Category(string name, Guid id) : this(name)
    {
        Id = id;
    }

    public static Category CreateCategory(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new MissingFieldException("Category name.");

        return new Category(name, id);
    }
}