namespace Tickette.Domain.Entities;

public class EmailUnsubscription
{
    public int Id { get; set; }

    public string Email { get; set; }

    public DateTime UnsubscribedAt { get; set; }

    public EmailUnsubscription(string email)
    {
        Email = email;
        UnsubscribedAt = DateTime.UtcNow;
    }
}