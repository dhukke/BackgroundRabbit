using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackgroundRabbit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BackgroundRabbit.MessageBroker
{
    public abstract class Consumer: IHostedService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        protected readonly ILogger Logger;
        protected string RouteKey { get; set; }
        protected  string QueueName { get; set; }

        protected Consumer(
            IConfiguration configuration,
            ILogger logger
        )
        {
            try
            {
                Logger = logger;

                var connectionStringConfiguration = configuration.GetSection("MessageBroker:Host");

                var factory = new ConnectionFactory()
                    {HostName = connectionStringConfiguration.Value};

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (Exception ex)
            {
                Logger.LogError($"RabbitListener init error,ex:{ex.Message}");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Register();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _connection.Close();
            return Task.CompletedTask;
        }

        public void Register()
        {
            Logger.LogInformation($"RabbitListener register,routeKey:{RouteKey}");

            DeclareExchange();

            DeclareQueue();

            BindExchangeToQueue();

            _channel.BasicQos(0, 2, false);

            ConfigureConsumer();
        }

        private void ConfigureConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var result = await Process(message);

                if (result)
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(queue: QueueName, consumer: consumer);
        }

        private void BindExchangeToQueue()
        {
            _channel.QueueBind(
                queue: QueueName,
                exchange: MessageConstants.FirstExchange,
                routingKey: RouteKey
            );
        }

        private void DeclareQueue()
        {
            _channel.QueueDeclare(
                queue: QueueName,
                exclusive: false
            );
        }

        private void DeclareExchange()
        {
            _channel.ExchangeDeclare(
                exchange: MessageConstants.FirstExchange,
                type: ExchangeType.Topic
            );
        }

        public void DeRegister()
        {
            this._connection.Close();
        }

        protected abstract Task<bool> Process(string message);
    }
}
