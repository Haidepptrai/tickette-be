using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces.Email;
using Tickette.Application.Common.Models.Email;

namespace Tickette.Infrastructure.Messaging.Feature;

public class EmailDispatcherConsumer : IConsumer<EmailWrapper>
{
    private readonly IServiceProvider _serviceProvider;

    public EmailDispatcherConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static readonly AsyncPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // 2s, 4s, 8s

    public async Task Consume(ConsumeContext<EmailWrapper> context)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var request = context.Message;
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            if (string.IsNullOrWhiteSpace(request.Type))
                return;

            switch (request.Type)
            {
                case EmailServiceKeys.EmailConfirm:
                    var confirmEmail = request.Payload.Deserialize<ConfirmEmailModel>();
                    if (confirmEmail != null)
                        await emailService.SendConfirmEmail(confirmEmail);
                    break;

                case EmailServiceKeys.AnnounceAddedMember:
                    var addedMemberEmail = request.Payload.Deserialize<AnnounceAddedMemberEmailModel>();
                    if (addedMemberEmail != null)
                        await emailService.SendAnnounceAddedMemberEmail(addedMemberEmail);
                    break;

                case EmailServiceKeys.EmailConfirmCreateOrder:
                    var orderEmail = request.Payload.Deserialize<OrderPaymentSuccessEmail>();
                    if (orderEmail != null)
                        await emailService.SendEmailAsync(orderEmail.BuyerEmail, "Order Process Successfully",
                            "ticket_order_confirmation", orderEmail);
                    break;

                default:
                    Console.WriteLine($"Unhandled email type: {request.Type}");
                    break;
            }
        });
    }
}