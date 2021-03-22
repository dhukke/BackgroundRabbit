using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace BackgroundRabbit
{
    public class PublisherWorker : BackgroundService
    {
        private readonly ILogger<PublisherWorker> _logger;
        private readonly IServiceProvider _provider;

        public PublisherWorker(ILogger<PublisherWorker> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PublisherWorker started running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _provider.CreateScope())
                {
                    var factory = scope.ServiceProvider.GetRequiredService<ConnectionFactory>();
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(
                            queue: MessageConstants.FirstQueue,
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null
                        );

                        var message = new Message
                        {
                            Id = Guid.NewGuid(),
                            Text = "Hello World!"
                        };

                        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                        channel.BasicPublish(
                            exchange: MessageConstants.FirstExchange,
                            routingKey: MessageConstants.FirstRountingKey,
                            basicProperties: null,
                            body: body
                        );

                        _logger.LogInformation("PublisherWorker [x] Sent {0}", message);

                    }
                }



                await Task.Delay(10000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            _logger.LogWarning("PublisherWorker SHUTDOWN");
            base.Dispose();
        }
    }
}
