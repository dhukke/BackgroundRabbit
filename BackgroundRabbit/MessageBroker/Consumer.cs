using BackgroundRabbit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundRabbit.MessageBroker;

public abstract class Consumer : IHostedService
{
    private IConnection _connection;
    private readonly IModel _channel;
    private readonly ConnectionFactory _factory;

    protected readonly ILogger _logger;
    protected string RouteKey { get; set; }
    protected string QueueName { get; set; }

    protected Consumer(
        IConfiguration configuration,
        ILogger logger
    )
    {
        try
        {
            _logger = logger;

            var connectionStringConfiguration = configuration.GetSection("MessageBroker:Host");

            _factory = new ConnectionFactory()
            {
                HostName = connectionStringConfiguration.Value
            };

            CreateConnection();

            _channel = _connection.CreateModel();
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogError("RabbitListener init error, BrokerUnreachableException: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("RabbitListener init error, ex: {Message}", ex.Message);
            throw;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Register(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Graceful shutdown
        _channel.Close();
        _connection.Close();

        return Task.CompletedTask;
    }

    public Task Register(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitListener register :{RouteKey}", RouteKey);

        if (_connection.IsOpen)
        {
            try
            {
                DeclareExchange();

                DeclareQueue();

                BindExchangeToQueue();

                _channel.BasicQos(0, 2, false);

                ConfigureConsumer(cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError("RabbitListener register :{RouteKey}", ex.Message);
                throw;
            }
        }

        return Task.CompletedTask;
    }

    private void DeclareExchange()
        => _channel.ExchangeDeclare(
            exchange: MessageConstants.FirstExchange,
            type: ExchangeType.Topic,
            durable: true
        );

    private void DeclareQueue()
        => _channel.QueueDeclare(
            queue: QueueName,
            exclusive: false,
            durable: true
        );

    private void BindExchangeToQueue()
        => _channel.QueueBind(
            queue: QueueName,
            exchange: MessageConstants.FirstExchange,
            routingKey: RouteKey
        );

    private void ConfigureConsumer(CancellationToken cancellationToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var result = await Process(message, cancellationToken);

            if (result)
            {
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel.BasicConsume(queue: QueueName, consumer: consumer);
    }

    public void DeRegister() => _connection.Close();

    protected abstract Task<bool> Process(string message, CancellationToken cancellationToken);
}
