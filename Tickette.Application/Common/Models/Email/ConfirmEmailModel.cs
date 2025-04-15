namespace Tickette.Application.Common.Models.Email;

public class ConfirmEmailModel : EmailTemplateModel
{
    public string UserEmail { get; init; }

    public string VerificationLink { get; set; }

    public string Token { get; init; }
}