namespace Tickette.Application.Features.AuditLogs.Common;

public class EventAuditLogDto
{
    public Guid Id { get; set; }

    public Guid EntityId { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

}