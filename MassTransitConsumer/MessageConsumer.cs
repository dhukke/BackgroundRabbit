using MassTransit;

namespace MassTransitConsumer;

public class MessageConsumer : IConsumer<Message>
{
    private readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Message> context)
    {
        _logger.LogInformation("Message: {@Message} consumed.", context.Message);

        return Task.CompletedTask;
    }
}
