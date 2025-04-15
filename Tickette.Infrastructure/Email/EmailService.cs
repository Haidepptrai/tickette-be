using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Tickette.Application.Common;
using Tickette.Application.Common.Interfaces.Email;
using Tickette.Application.Common.Models;
using Tickette.Application.Common.Models.Email;
using Tickette.Domain.Entities;
using Tickette.Infrastructure.Data;

namespace Tickette.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ApplicationDbContext _dbContext;

    public EmailService(IOptions<EmailSettings> emailSettings, ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> SendEmailAsync<T>(string to, string subject, string templateName, T model) where T : EmailTemplateModel
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            string token = GenerateUnsubscribeToken(to);

            // Ensure UnsubscribeLink is always included
            model.UnsubscribeLink = $"{_emailSettings.BaseUrl}/api/email/unsubscribe?email={to}&token={token}";

            // Load email template
            string emailBody = LoadEmailTemplate(templateName, model);

            var bodyBuilder = new BodyBuilder { HtmlBody = emailBody };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendConfirmEmail(ConfirmEmailModel model)
    {
        var encodedToken = WebUtility.UrlEncode(model.Token);
        model.VerificationLink = $"{_emailSettings.ClientUrl}/profile/email-confirmation?email={model.UserEmail}&token={encodedToken}";

        return await SendTemplatedEmailAsync(
            recipientName: model.UserEmail ?? "", // Optional
            recipientEmail: model.UserEmail,
            subject: "Confirm Your Email",
            templateName: "confirm_email",
            model: model
        );
    }

    public async Task<bool> SendAnnounceAddedMemberEmail(AnnounceAddedMemberEmailModel model)
    {
        return await SendTemplatedEmailAsync(
            recipientName: model.RecipientName,
            recipientEmail: model.RecipientEmail,
            subject: "You have been added as a staff!",
            templateName: "event_role_promotion",
            model: model
        );
    }

    private async Task<bool> SendTemplatedEmailAsync<TModel>(
        string recipientName,
        string recipientEmail,
        string subject,
        string templateName,
        TModel model)
        where TModel : EmailTemplateModel
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(new MailboxAddress(recipientName ?? string.Empty, recipientEmail));
        email.Subject = subject;

        // Load template
        string emailBody = LoadEmailTemplate(templateName, model);

        var bodyBuilder = new BodyBuilder { HtmlBody = emailBody };
        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        return true;
    }

    public async Task<bool> UnsubscribeEmailAsync(string email)
    {
        bool isAlreadyUnsubscribed = await _dbContext.EmailUnsubscriptions.AnyAsync(e => e.Email == email);
        if (isAlreadyUnsubscribed)
            return false; // Already unsubscribed

        var unsubscription = new EmailUnsubscription(email);
        _dbContext.EmailUnsubscriptions.Add(unsubscription);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsEmailUnsubscribedAsync(string email)
    {
        return await _dbContext.EmailUnsubscriptions.AnyAsync(e => e.Email == email);
    }

    public string GenerateUnsubscribeToken(string email)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("YourSecretKey"));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(email));
        return Convert.ToBase64String(hash);
    }

    public bool ValidateUnsubscribeToken(string email, string token)
    {
        string expectedToken = GenerateUnsubscribeToken(email);
        return expectedToken == token;
    }

    private static string LoadEmailTemplate<T>(string templateName, T model) where T : EmailTemplateModel
    {
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Emails", "Templates", $"{templateName}.html");
        string headerPath = Path.Combine(Directory.GetCurrentDirectory(), "Emails", "Templates", "Partials", "header.html");
        string footerPath = Path.Combine(Directory.GetCurrentDirectory(), "Emails", "Templates", "Partials", "footer.html");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Email template '{templateName}' not found at {templatePath}");
        if (!File.Exists(headerPath) || !File.Exists(footerPath))
            throw new FileNotFoundException("Header or Footer template not found.");

        // Load Templates
        string emailBody = File.ReadAllText(templatePath);
        string header = File.ReadAllText(headerPath);
        string footer = File.ReadAllText(footerPath);

        // Insert Header & Footer
        emailBody = emailBody.Replace("{{ Header }}", header);
        emailBody = emailBody.Replace("{{ Footer }}", footer);

        // Get all properties of the model and replace placeholders
        PropertyInfo[] properties = model.GetType().GetProperties();
        foreach (var prop in properties)
        {
            string placeholder = "{{" + prop.Name + "}}";
            string value = prop.GetValue(model)?.ToString() ?? "";
            emailBody = emailBody.Replace(placeholder, value);
        }

        return emailBody;
    }
}
