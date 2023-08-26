using BackgroundRabbit.Producer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;

namespace BackgroundRabbit.Producer.MessageBroker;

public class Producer : IProducer
{

    private readonly ILogger _logger;
    private readonly ConnectionFactory _factory;

    private IModel? _channel;
    private IConnection? _connection;

    public Producer(
        IConfiguration configuration,
        ILogger<Producer> logger
    )
    {
        _logger = logger;

        var connectionStringConfiguration = configuration.GetSection("MessageBroker:Host");

        _factory = new ConnectionFactory()
        {
            HostName = connectionStringConfiguration.Value
        };

        try
        {
            CreateConnection();

            _channel = _connection!.CreateModel();
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogError("RabbitListener init error, BrokerUnreachableException: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(-1, ex, "RabbitMQClient init fail");
        }
    }

    private void CreateConnection()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromMilliseconds(35),
            retryCount: 3
        );

        var retryPolicy = Policy
            .Handle<BrokerUnreachableException>()
            .WaitAndRetry(delay);

        retryPolicy.Execute(() =>
        {
            _connection = _factory.CreateConnection();
        });
    }

    private void CreateModel()
    {
        if (_channel is not null && _channel.IsOpen)
        {
            return;
        }

        var delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromMilliseconds(35),
            retryCount: 1
        );

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(delay);

        retryPolicy.Execute(() =>
        {
            _channel = _connection!.CreateModel();
        });
    }

    public void PushMessage(
        string routingKey,
        object message
    )
    {
        _logger.LogInformation("PushMessage to routingKey: {routingKey}", routingKey);

        var jsonMessage = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        CreateModel();

        if (_channel is null || _channel.IsClosed)
        {
            _logger.LogWarning("Channel is null or closed, not possible to pushMessage to routingKey: {routingKey}", routingKey);
            return;
        }

        _channel.BasicPublish(
            exchange: MessageConstants.FirstExchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body
        );
    }
}
