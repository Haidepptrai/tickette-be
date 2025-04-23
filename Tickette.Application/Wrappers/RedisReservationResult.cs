namespace Tickette.Infrastructure.Messaging;

public class RedisReservationResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; } // Optional: "SeatConflict", "InventoryMissing", etc.
    public string? Message { get; set; }   // Human-readable explanation

    public static RedisReservationResult Ok() => new() { Success = true };
    public static RedisReservationResult Fail(string message, string? code = null) =>
        new() { Success = false, Message = message, ErrorCode = code };

}