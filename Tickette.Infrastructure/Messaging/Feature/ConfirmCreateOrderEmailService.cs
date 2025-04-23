using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces.Email;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Models.Email;

namespace Tickette.Infrastructure.Messaging.Feature;

public class ConfirmCreateOrderEmailService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageConsumer _messageConsumer;

    public ConfirmCreateOrderEmailService(IServiceProvider serviceProvider, IMessageConsumer messageConsumer)
    {
        _serviceProvider = serviceProvider;
        _messageConsumer = messageConsumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.ConsumeAsync(EmailServiceKeys.EmailConfirmCreateOrder, async (message) =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var emailPayload = JsonSerializer.Deserialize<OrderPaymentSuccessEmail>(message);

                if (emailPayload is null)
                {
                    Console.WriteLine("error while sending email");
                    return null;
                }

                await emailService.SendEmailAsync(emailPayload.BuyerEmail, "Order Process Successfully",
                    "ticket_order_confirmation", emailPayload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending email: {ex.Message}");
            }
            return null;
        }, stoppingToken);

    }
}