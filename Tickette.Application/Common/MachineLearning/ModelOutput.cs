namespace Tickette.Application.Common.MachineLearning;

public class ModelOutput
{
    public Guid EventId { get; set; }

    public string City { get; set; }

    public string District { get; set; }

    public string Ward { get; set; }

    public string StreetAddress { get; set; }

    public decimal RecommendedTicketPrice { get; set; }

    public float Score { get; set; }
}