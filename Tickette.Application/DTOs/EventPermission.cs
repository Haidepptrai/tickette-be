namespace Tickette.Application.DTOs;

public class EventPermission
{
    public Guid EventId { get; set; }
    public string RoleName { get; set; }
    public List<string> Permissions { get; set; }
}