namespace Tickette.Application.Wrappers;

public class RabbitMQMessageResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; } // Optional: "SeatConflict", "InventoryMissing", etc.
    public string? Message { get; set; }   // Human-readable explanation

    public static RabbitMQMessageResult Ok() => new() { Success = true };
    public static RabbitMQMessageResult Fail(string message, string? code = null) =>
        new() { Success = false, Message = message, ErrorCode = code };

}