namespace Tickette.Domain.Entities;

public sealed class EventSeatMapSection
{
    public double X { get; private set; }

    public double Y { get; private set; }

    public double Width { get; private set; }

    public double Height { get; private set; }

    public string Name { get; private set; }

    public string Type { get; private set; } // Stage or Sections

    public string Fill { get; private set; }

    public string Stroke { get; private set; }

    public int StrokeWidth { get; private set; }

    public int CornerRadius { get; private set; }

    public Guid TicketId { get; private set; }

    public int TicketQuantity { get; private set; }

    public EventSeatMapSection(
        double x, double y, double width, double height, string name, string type,
        string fill, string stroke, int strokeWidth, int cornerRadius,
        Guid ticketId, int ticketQuantity)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Name = name;
        Type = type;
        Fill = fill;
        Stroke = stroke;
        StrokeWidth = strokeWidth;
        CornerRadius = cornerRadius;
        TicketId = ticketId;
        TicketQuantity = ticketQuantity;
    }

    public static EventSeatMapSection CreateEventSeatMapSection(
        double x, double y, double width, double height, string name, string type,
        string fill, string stroke, int strokeWidth, int cornerRadius,
        Guid ticketId, int ticketQuantity)
    {
        return new EventSeatMapSection(x, y, width, height, name, type, fill, stroke, strokeWidth, cornerRadius, ticketId, ticketQuantity);
    }
}
