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

    // This key is used to store the reservation info for how many quantity reduce
    // for reservation with seats
    public static string GetSeatsReservationKey(Guid ticketId, Guid userId) =>
        $"{Prefix}:seat_reservation_info:{ticketId}:{userId}";

    public static string GetReservedSeatKey(Guid ticketId, Guid userId, string rowName, string seatNumber) =>
        $"{Prefix}:seat_reservation:{ticketId}:{userId}:seat:{rowName}:{seatNumber}";

    public static string GetLockSeat(Guid ticketId, SeatOrder seat) =>
        $"lock:reserve:{ticketId}:{seat.RowName}{seat.SeatNumber}";

    public static string GetBookedSeatKey(Guid ticketId, string row, string seat) =>
        $"booked:{ticketId}:seat:{row}-{seat}";
}
