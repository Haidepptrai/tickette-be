using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces.Email;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Models.Email;

namespace Tickette.Infrastructure.Messaging.Feature;

public class ConfirmEmailServiceConsumer : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;

    public ConfirmEmailServiceConsumer(IMessageConsumer messageConsumer, IServiceProvider serviceProvider)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.ConsumeAsync(EmailServiceKeys.EmailConfirm, async (message) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var email = JsonSerializer.Deserialize<ConfirmEmailModel>(message);

            if (email == null)
            {
                return;
            }

            await emailService.SendConfirmEmail(email);
        }, stoppingToken);
    }
}