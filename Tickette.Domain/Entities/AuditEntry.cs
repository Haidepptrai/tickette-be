namespace Tickette.Domain.Entities;

public class AuditEntry
{
    public string TableName { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string Action { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public Guid UserId { get; set; }

    public Dictionary<string, object> Changes { get; set; } = new();
}