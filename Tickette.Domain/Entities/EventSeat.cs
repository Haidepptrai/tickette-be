using System.Text.Json.Serialization;

namespace Tickette.Domain.Entities;

public sealed class EventSeat
{
    [JsonPropertyName("height")]
    public int Height { get; private set; }

    [JsonPropertyName("width")]
    public int Width { get; private set; }

    [JsonPropertyName("x")]
    public double X { get; private set; }

    [JsonPropertyName("y")]
    public double Y { get; private set; }

    [JsonPropertyName("number")]
    public string Number { get; private set; }

    [JsonPropertyName("rowName")]
    public string RowName { get; private set; }

    [JsonPropertyName("isOrdered")]
    public bool IsOrdered { get; private set; }

    [JsonPropertyName("ticketId")]
    public Guid TicketId { get; private set; }

    [JsonPropertyName("groupId")]
    public string GroupId { get; private set; } // It is used to group seats in the same row, for UI view only

    [JsonConstructor]
    public EventSeat(double x, double y, int width, int height, string number, string rowName, Guid ticketId, string groupId, bool isOrdered)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Number = number;
        RowName = rowName;
        IsOrdered = isOrdered;
        TicketId = ticketId;
        GroupId = groupId;
    }

    public void SetIsOrdered()
    {
        IsOrdered = true;
    }
}