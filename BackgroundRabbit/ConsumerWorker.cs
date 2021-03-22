using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BackgroundRabbit
{
    public class ConsumerWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceProvider _provider;

        public ConsumerWorker(ILoggerFactory loggerFactory, IServiceProvider provider)
        {
            _logger = loggerFactory.CreateLogger<ConsumerWorker>();
            _provider = provider;
            InitRabbitMq();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumerWorker running at: {Time}", DateTimeOffset.Now);

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ch, ea) =>
            {
                _logger.LogInformation("ConsumerWorker message received at: {Time}", DateTimeOffset.Now);

                var body = ea.Body.ToArray();
                var content = Encoding.UTF8.GetString(body);

                using (var scope = _provider.CreateScope())
                {
                    var consumerHandler = scope.ServiceProvider.GetRequiredService<ConsumerHandler>();

                    var content1 = JsonConvert.DeserializeObject<Message>(content);
                    consumerHandler.HandleMessage(content1);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(MessageConstants.FirstQueue, false, consumer);
            return Task.CompletedTask;
        }

        private void InitRabbitMq()
        {
            _logger.LogInformation("ConsumerWorker InitRabbitMq");

            using (var scope = _provider.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<ConnectionFactory>();

                _connection = factory.CreateConnection();

                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(MessageConstants.FirstExchange, ExchangeType.Topic);

                _channel.QueueDeclare(
                    MessageConstants.FirstQueue,
                    false,
                    false,
                    false,
                    null
                );

                _channel.QueueBind(
                    MessageConstants.FirstQueue,
                    MessageConstants.FirstExchange,
                    MessageConstants.FirstRountingKey,
                    null
                );
                _channel.BasicQos(0, 1, false);
            }
        }

        public override void Dispose()
        {
            _logger.LogInformation("ConsumerWorker SHUTDOWN");
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
