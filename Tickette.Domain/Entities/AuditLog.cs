namespace Tickette.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public string TableName { get; private set; }
    public Guid EntityId { get; private set; }
    public string Action { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Guid UserId { get; private set; }
    public string Data { get; private set; }

    private AuditLog() { }

    public AuditLog(Guid entityId, string tableName, string action, DateTime timestamp, Guid userId, string data)
    {
        Id = Guid.NewGuid(); // or pass it from outside if needed
        EntityId = entityId;
        TableName = tableName;
        Action = action;
        Timestamp = timestamp;
        UserId = userId;
        Data = data;
    }
}
