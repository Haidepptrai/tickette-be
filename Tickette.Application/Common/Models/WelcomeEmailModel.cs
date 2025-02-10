namespace Tickette.Application.Common.Models;

public class WelcomeEmailModel : EmailTemplateModel
{
    public string UserName { get; set; }
    public string VerificationLink { get; set; }
}