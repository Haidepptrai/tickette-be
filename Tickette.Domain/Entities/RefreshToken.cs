namespace Tickette.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiryTime { get; private set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public RefreshToken(string token, DateTime expiryTime)
    {
        Id = Guid.NewGuid();
        Token = token;
        ExpiryTime = expiryTime;
    }
}