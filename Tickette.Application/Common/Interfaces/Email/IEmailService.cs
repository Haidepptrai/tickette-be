using Tickette.Application.Common.Models;
using Tickette.Application.Common.Models.Email;

namespace Tickette.Application.Common.Interfaces.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync<T>(string to, string subject, string templateName, T model) where T : EmailTemplateModel;

    Task<bool> UnsubscribeEmailAsync(string email);

    Task<bool> IsEmailUnsubscribedAsync(string email);

    string GenerateUnsubscribeToken(string email);

    bool ValidateUnsubscribeToken(string email, string token);

    Task<bool> SendConfirmEmail(ConfirmEmailModel model);
}