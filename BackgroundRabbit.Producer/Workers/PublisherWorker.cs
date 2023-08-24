using BackgroundRabbit.Producer.Entities;
using BackgroundRabbit.Producer.MessageBroker;
using BackgroundRabbit.Producer.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundRabbit.Producer.Workers
{
    public class PublisherWorker : BackgroundService
    {
        private readonly ILogger<PublisherWorker> _logger;
        private readonly IProducer _producer;

        public PublisherWorker(
            ILogger<PublisherWorker> logger,
            IProducer producer
        )
        {
            _logger = logger;
            _producer = producer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PublisherWorker started running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                _producer.PushMessage(
                    MessageConstants.FirstRountingKey,
                    new Message
                    {
                        Id = Guid.NewGuid(),
                        Content = $"Hello World! {DateTime.UtcNow}",
                        DateTime = DateTime.UtcNow
                    }
                );

                await Task.Delay(30000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            _logger.LogWarning("PublisherWorker SHUTDOWN");
            base.Dispose();
        }
    }
}
