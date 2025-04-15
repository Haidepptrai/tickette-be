namespace Tickette.Application.Common.Models.Email;

public class AnnounceAddedMemberEmailModel : EmailTemplateModel
{
    public string RecipientName { get; set; }

    public string RecipientEmail { get; set; }

    public string EventName { get; set; }

    public string Role { get; set; }

    public string EventLink { get; set; }

    public AnnounceAddedMemberEmailModel(string recipientName, string recipientEmail, string eventName, string role, string eventLink)
    {
        RecipientName = recipientName;
        RecipientEmail = recipientEmail;
        EventName = eventName;
        Role = role;
        EventLink = eventLink;
    }
}