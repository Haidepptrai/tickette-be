namespace Tickette.Infrastructure.Prediction.Models;

public sealed class EventDetailsModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string EventDescription { get; set; }
    public string Category { get; set; }
    public string Organizer { get; set; }
    public string Location { get; set; }
    public DateTime Datetime { get; set; }
    public string TicketPriceRange { get; set; }

    public EventDetailsModel(string title, string eventDescription, string category,
        string organizer, string location, DateTime datetime, string ticketPriceRange)
    {
        Id = Guid.NewGuid();
        Title = title;
        EventDescription = eventDescription;
        Category = category;
        Organizer = organizer;
        Location = location;
        Datetime = datetime;
        TicketPriceRange = ticketPriceRange;
    }
}