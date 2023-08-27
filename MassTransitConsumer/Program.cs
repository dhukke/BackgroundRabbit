// See https://aka.ms/new-console-template for more information
using MassTransit;

var builder = WebApplication.CreateBuilder();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.SetInMemorySagaRepositoryProvider();

    var assembly = typeof(Program).Assembly;

    x.AddConsumers(assembly);
    x.AddSagaStateMachines(assembly);
    x.AddSagas(assembly);
    x.AddActivities(assembly);


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

var app = builder.Build();

app.Run("http://127.0.0.1:5002");
