namespace Tickette.Application.Common.Models;

public class OrderPaymentSuccessEmail : EmailTemplateModel
{
    public string BuyerName { get; set; }

    public string BuyerEmail { get; set; }

    public string BuyerPhone { get; set; }

    public string EventName { get; set; }

    public string EventDate { get; set; }

    public string EventTime { get; set; }

    public string EventLocation { get; set; }

    public string TicketDetails { get; set; } // This will be a formatted HTML table

    public string TicketDownloadLink { get; set; }

    public string SupportLink { get; set; } = "https://localhost:3000/support";

    public OrderPaymentSuccessEmail(string buyerName, string buyerEmail, string buyerPhone, string eventName, string eventDate, string eventTime, string eventLocation, string ticketDetails, string ticketDownloadLink)
    {
        BuyerName = buyerName;
        BuyerEmail = buyerEmail;
        BuyerPhone = buyerPhone;
        EventName = eventName;
        EventDate = eventDate;
        EventTime = eventTime;
        EventLocation = eventLocation;
        TicketDetails = ticketDetails;
        TicketDownloadLink = $"http://localhost:3000 + ${ticketDownloadLink}";
    }
}
