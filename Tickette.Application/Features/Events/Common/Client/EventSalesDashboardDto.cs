using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Common.Client;

public class EventSalesDashboardDto
{
    public Guid EventId { get; init; }
    
    public string EventName { get; init; }
    
    public decimal TotalRevenue { get; init; }
    
    public string Currency { get; init; }
    
    public int TotalTicketsSold { get; init; }
    
    public IEnumerable<TicketTypeSalesDto> TicketTypeSales { get; init; }
}

public class TicketTypeSalesDto
{
    public Guid TicketTypeId { get; init; }
    
    public string TicketTypeName { get; init; }
    
    public int QuantitySold { get; init; }
    
    public decimal Revenue { get; init; }
} 