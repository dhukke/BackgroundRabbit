// See https://aka.ms/new-console-template for more information
using MassTransit;
using MassTransitProducer;

var builder = WebApplication.CreateBuilder();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            cfg.Host("localhost", "/", hostConfigurator =>
            {
                hostConfigurator.Username("guest");
                hostConfigurator.Password("guest");
            });
            cfg.ConfigureEndpoints(context);
        }
    );
});

builder.Services.AddHostedService<MessagePublisher>();

var app = builder.Build();

app.Run();
