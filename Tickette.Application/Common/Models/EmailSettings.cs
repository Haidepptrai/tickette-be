namespace Tickette.Application.Common.Models;

public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; }
    public string SenderName { get; set; }
    public string SenderPassword { get; set; }
    public bool UseSsl { get; set; }
    public bool UseTls { get; set; }
    public string BaseUrl { get; set; }
    public string UnsubscribeSecretKey { get; set; }
}