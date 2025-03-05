namespace Tickette.Infrastructure.Helpers;

public static class RedisKeys
{
    private const string Prefix = "tickette"; // General prefix for all keys

    public static string GetTicketQuantityKey(Guid ticketId) =>
        $"{Prefix}:ticket:{ticketId}:remaining_tickets";

    public static string GetReservationKey(Guid ticketId, Guid userId) =>
        $"{Prefix}:reservation:{ticketId}:{userId}";


}
