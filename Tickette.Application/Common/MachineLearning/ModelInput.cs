namespace Tickette.Application.Common.MachineLearning;

public class ModelInput
{
    public Guid UserId { get; set; }

    public Guid EventId { get; set; }

    public Guid TicketId { get; set; }
}