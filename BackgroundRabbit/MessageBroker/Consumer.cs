using BackgroundRabbit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundRabbit.MessageBroker;

public abstract class Consumer : IHostedService
{
    private IConnection _connection;
    private IModel _channel;
    private bool _connectionBlocked;
    private AsyncRetryPolicy _retryPolicy;
    private ConnectionFactory _factory;

    protected readonly ILogger Logger;
    protected string RouteKey { get; set; }
    protected string QueueName { get; set; }

    protected Consumer(
        IConfiguration configuration,
        ILogger logger
    )
    {
        try
        {
            Logger = logger;

            var connectionStringConfiguration = configuration.GetSection("MessageBroker:Host");

            _factory = new ConnectionFactory()
            {
                HostName = connectionStringConfiguration.Value
            };

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _connection.ConnectionBlocked += ConnectionBlockedHandler;
            _connection.ConnectionUnblocked += ConnectionUnblockedHandler;

            // check backoff 
            _retryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, calculatedWaitDuration, retryCount, context) =>
                {
                    // Log the reconnection attempt
                });
        }
        catch (Exception ex)
        {
            Logger.LogError("RabbitListener init error, ex: {Message}", ex.Message);
        }
    }

    private void ConnectionBlockedHandler(object sender, ConnectionBlockedEventArgs e)
    {
        _connectionBlocked = true;
        // Log the connection blocked event
    }

    private void ConnectionUnblockedHandler(object sender, EventArgs e)
    {
        _connectionBlocked = false;
        // Log the connection unblocked event
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

    public async Task Register(CancellationToken cancellationToken)
    {
        Logger.LogInformation("RabbitListener register :{RouteKey}", RouteKey);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_connection.IsOpen || _connectionBlocked)
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    // Attempt to reconnect
                    _connection.Dispose();
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _connectionBlocked = false;
                });
            }

            if (_connection.IsOpen && !_connectionBlocked)
            {
                DeclareExchange();

                DeclareQueue();

                BindExchangeToQueue();

                _channel.BasicQos(0, 2, false);

                ConfigureConsumer(cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private void DeclareExchange()
        => _channel.ExchangeDeclare(
            exchange: MessageConstants.FirstExchange,
            type: ExchangeType.Topic
        );

    private void DeclareQueue()
        => _channel.QueueDeclare(
            queue: QueueName,
            exclusive: false
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
