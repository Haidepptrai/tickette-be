using System.Text.Json.Serialization;

namespace Tickette.Domain.Entities;

public sealed class EventSeat
{
    [JsonPropertyName("height")]
    public string Height { get; private set; }

    [JsonPropertyName("width")]
    public string Width { get; private set; }

    [JsonPropertyName("x")]
    public double X { get; private set; }

    [JsonPropertyName("y")]
    public double Y { get; private set; }

    [JsonPropertyName("number")]
    public string Number { get; private set; }

    [JsonPropertyName("rowName")]
    public string RowName { get; private set; }

    [JsonPropertyName("fill")]
    public string Fill { get; private set; }

    [JsonPropertyName("isOrdered")]
    public bool IsOrdered { get; private set; }

    [JsonConstructor]
    public EventSeat(double x, double y, string width, string height, string number, string rowName, string fill)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Number = number;
        RowName = rowName;
        Fill = fill;
        IsOrdered = false;
    }

    public static EventSeat CreateEventSeat(double x, double y, string width, string height, string number, string rowName, string fill)
    {
        return new EventSeat(x, y, width, height, number, rowName, fill);
    }

    public void SetIsOrdered()
    {
        IsOrdered = true;
    }
}