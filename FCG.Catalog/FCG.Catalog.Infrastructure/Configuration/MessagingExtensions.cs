using FCG.Catalog.Application.Common.Ports;
using FCG.Catalog.Application.Events;
using FCG.Catalog.Infrastructure.Adapters.Events;
using FCG.Catalog.Infrastructure.Adapters.Events.Consumers;
using FCG.Payments.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Catalog.Infrastructure.Configuration;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentProcessedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.Message<PaymentProcessedEvent>(m =>
                {
                    m.SetEntityName("payment-processed");
                });

                cfg.Message<OrderPlacedEvent>(m =>
                {
                    m.SetEntityName("order-placed");
                });

                cfg.ReceiveEndpoint("payment-processed-catalog-queue", e =>
                {
                    e.ConfigureConsumer<PaymentProcessedConsumer>(context);
                });

            });
        });

        return services;
    }
}