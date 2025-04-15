namespace Tickette.Infrastructure.Prediction.Models;

public class ModelLogs
{
    public Guid Id { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}