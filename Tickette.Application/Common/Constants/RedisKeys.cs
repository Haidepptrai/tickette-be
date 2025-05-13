using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Constants;

public static class RedisKeys
{
    private const string Prefix = "Tickette"; // General prefix for all keys

    public static string GetTicketQuantityKey(Guid ticketId) =>
        $"{Prefix}:ticket:{ticketId}:remaining_tickets";

    // This key is used to store the reservation info for how many quantity reduce
    // for reservation without seats
    public static string GetReservationKey(Guid ticketId, Guid userId) =>
        $"{Prefix}:reservation:{ticketId}:{userId}";

    // This key will store user’s full reservation metadata
    public static string GetUserTicketReservationKey(Guid ticketId, Guid userId) =>
        $"{Prefix}:seat_reservation:{ticketId}:{userId}";

    // Tracks all currently reserved seat labels for a specific ticket
    public static string GetTicketReservedSeatsKey(Guid ticketId) =>
        $"{Prefix}:ticket_reserved_seats:{ticketId}";

    // This key is used to store the lock for a specific seat
    public static string GetLockedSeat(Guid ticketId, SeatOrder seat) =>
        $"{Prefix}:lock:reserve:{ticketId}:{seat.RowName}{seat.SeatNumber}";

    public static string GetBookedSeatKey(Guid ticketId, string row, string seat) =>
        $"booked:{ticketId}:seat:{row}-{seat}";

    public static long GetBookedSeatExpireTimeInSeconds() => 15 * 60; // 15 minutes
}
