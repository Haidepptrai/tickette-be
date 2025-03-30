namespace Tickette.Application.Common.Constants;

public static class InMemoryCacheKey
{
    public static string CommitteeMemberOfEvent(Guid eventId) => $"CommitteeMemberOfEvent-{eventId}";

    public static string CouponList(Guid eventId) => $"CouponList-{eventId}";

}