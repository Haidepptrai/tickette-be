using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;

namespace Tickette.Application.Factories;

public static class TicketFactory
{
    public static async Task<Ticket> CreateTicketAsync(
        EventDate eventDate,
        string name,
        decimal price,
        int totalTickets,
        int minTicketsPerOrder,
        int maxTicketsPerOrder,
        DateTime saleStartTime,
        DateTime saleEndTime,
        string description,
        IFileUpload? ticketImageFile,
        IFileUploadService fileUploadService)
    {
        // Upload the ticket image and get the URL
        var ticketImageUrl = ticketImageFile is null ? "" : await fileUploadService.UploadFileAsync(ticketImageFile, "tickets");

        // Create and return the Ticket
        return Ticket.Create(
            eventDate,
            name: name,
            price: price,
            totalTickets: totalTickets,
            minTicketsPerOrder: minTicketsPerOrder,
            maxTicketsPerOrder: maxTicketsPerOrder,
            saleStartTime: saleStartTime,
            saleEndTime: saleEndTime,
            description: description,
            ticketImage: ticketImageUrl
        );
    }
}
