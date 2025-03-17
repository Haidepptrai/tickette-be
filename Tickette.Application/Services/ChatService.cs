using Tickette.Application.Common.Interfaces.SignalR;

namespace Tickette.Application.Services;

public class ChatService : IChatService
{
    public Task<Guid> CreateGuestSessionAsync(string name, string email)
    {
        throw new NotImplementedException();
    }
}