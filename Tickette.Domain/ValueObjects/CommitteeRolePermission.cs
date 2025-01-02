using Tickette.Domain.Common;

namespace Tickette.Domain.ValueObjects;

public class CommitteeRolePermission : ValueObject
{
    public string Name { get; private set; }

    public CommitteeRolePermission(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty.");

        Name = name;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}