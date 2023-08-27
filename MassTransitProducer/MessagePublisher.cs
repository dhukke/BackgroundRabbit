using MassTransit;
using MassTransitConsumer;

namespace MassTransitProducer;

public class MessagePublisher : BackgroundService
{
    private readonly ILogger<MessagePublisher> _logger;
    private readonly IBus _bus;

    public MessagePublisher(ILogger<MessagePublisher> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Yield();

            var message = new Message(DateTime.UtcNow.ToString());

            await _bus.Publish(message);

            _logger.LogInformation("Message: {@Message} was published.", message);

            await Task.Delay(5000, stoppingToken);
        }
    }
}
