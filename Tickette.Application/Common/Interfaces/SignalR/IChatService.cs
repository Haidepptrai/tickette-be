namespace Tickette.Application.Common.Interfaces.SignalR;

public interface IChatService
{
    Task<Guid> CreateGuestSessionAsync(string name, string email);

}