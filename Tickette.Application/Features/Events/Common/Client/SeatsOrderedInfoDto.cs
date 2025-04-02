namespace Tickette.Application.Features.Events.Common.Client;

public record SeatsOrderedInfoDto
{
    public string RowName { get; set; }
    public string SeatNumber { get; set; }
}