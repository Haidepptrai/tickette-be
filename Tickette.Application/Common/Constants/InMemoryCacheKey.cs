namespace Tickette.Application.Common.Constants;

public static class InMemoryCacheKey
{
    private const string Suffix = "Cache";

    public static string CommitteeMemberOfEvent(Guid eventId) => $"CommitteeMemberOfEvent-{eventId}-{Suffix}";

    public static string CouponList(Guid eventId) => $"CouponList-{eventId}-{Suffix}";

    public static string CategoryList() => $"CategoryList-{Suffix}";
}