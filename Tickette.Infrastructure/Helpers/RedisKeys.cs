namespace Tickette.Infrastructure.Helpers;

public static class RedisKeys
{
    private const string Prefix = "tickette"; // General prefix for all keys

    public static string GetReservationKey(Guid reservationId, Guid userId) =>
        $"{Prefix}:reservation:{reservationId}:{userId}";

    public static string GetUserSessionKey(Guid userId) =>
        $"{Prefix}:user_session:{userId}";

    public static string GetEventSeatMapKey(Guid eventId) =>
        $"{Prefix}:event_seat_map:{eventId}";

    public static string GetTicketAvailabilityKey(Guid ticketId) =>
        $"{Prefix}:ticket_availability:{ticketId}";

    public static string GetPaymentStatusKey(Guid paymentId) =>
        $"{Prefix}:payment_status:{paymentId}";

    public static string GetCacheKey(RedisKeyPrefix keyPrefix, params object[] values) =>
        $"{Prefix}:{keyPrefix.ToString().ToLower()}:{string.Join(":", values)}";
}
