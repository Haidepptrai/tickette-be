namespace Tickette.Domain.Common;

public record Unit
{
    public static Unit Value { get; } = new Unit();
    private Unit() { }
}