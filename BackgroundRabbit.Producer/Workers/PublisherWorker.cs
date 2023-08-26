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
                try
                {
                    var message = new Message
                    {
                        Id = Guid.NewGuid(),
                        Content = $"Hello World! {DateTime.UtcNow}",
                        DateTime = DateTime.UtcNow
                    };

                    _producer.PushMessage(
                        MessageConstants.FirstRountingKey,
                        message
                    );

                    _logger.LogInformation("Publish message: {@Message}", message);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            _logger.LogWarning("PublisherWorker SHUTDOWN");
            base.Dispose();
        }
    }
}
