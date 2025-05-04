using MassTransit;
using Tickette.Application.Common.Constants;
using Tickette.Infrastructure.Messaging.Feature;
using Tickette.Infrastructure.Settings;

namespace Tickette.Infrastructure.Extensions;

// Declare this one because later we can expand it to WorkerService project
public static class MassTransitConfiguration
{
    public static void ConfigureConsumers(IBusRegistrationConfigurator x)
    {
        x.AddConsumer<EmailDispatcherConsumer>();
        x.AddConsumer<ReserveTickerConsumer>();
        x.AddConsumer<RemoveTicketReservationConsumer>();
        x.AddConsumer<ConfirmTicketReservationConsumer>();
    }

    public static void ConfigureRabbitMq(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg, RabbitMQSettings rabbitMQSettings)
    {
        cfg.Host(rabbitMQSettings.HostName, "/", h =>
        {
            h.Username(rabbitMQSettings.UserName);
            h.Password(rabbitMQSettings.Password);
        });

        cfg.ReceiveEndpoint("email-dispatcher", e =>
        {
            e.ConfigureConsumer<EmailDispatcherConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });

        cfg.ReceiveEndpoint(MessageQueueKeys.TicketReservationCreated, e =>
        {
            e.PrefetchCount = 300;
            e.ConcurrentMessageLimit = 300;
            e.ConfigureConsumer<ReserveTickerConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });

        cfg.ReceiveEndpoint(MessageQueueKeys.TicketReservationCancelled, e =>
        {
            e.ConfigureConsumer<RemoveTicketReservationConsumer>(context);
        });

        cfg.ReceiveEndpoint(MessageQueueKeys.TicketReservationConfirmed, e =>
        {
            e.ConfigureConsumer<ConfirmTicketReservationConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });
    }
}
