using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces.Email;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Models.Email;

namespace Tickette.Infrastructure.Messaging.Feature;

public class EmailServiceConsumer : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;

    public EmailServiceConsumer(IMessageConsumer messageConsumer, IServiceProvider serviceProvider)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.ConsumeAsync(EmailServiceKeys.Email, async (message) =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var wrapper = JsonSerializer.Deserialize<EmailWrapper>(message);
                if (wrapper == null || string.IsNullOrWhiteSpace(wrapper.Type))
                    return null;

                switch (wrapper.Type)
                {
                    case EmailServiceKeys.EmailConfirm:
                        var confirmEmail = wrapper.Payload.Deserialize<ConfirmEmailModel>();
                        if (confirmEmail != null)
                            await emailService.SendConfirmEmail(confirmEmail);
                        break;

                    case EmailServiceKeys.AnnounceAddedMember:
                        var addedMemberEmail = wrapper.Payload.Deserialize<AnnounceAddedMemberEmailModel>();
                        if (addedMemberEmail != null)
                            await emailService.SendAnnounceAddedMemberEmail(addedMemberEmail);
                        break;

                    default:
                        Console.WriteLine($"Unhandled email type: {wrapper.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error while sending email: {ex.Message}");
            }

            return null;
        }, stoppingToken);
    }
}