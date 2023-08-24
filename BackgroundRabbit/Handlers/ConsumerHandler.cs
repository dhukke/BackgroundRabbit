using BackgroundRabbit.Database;
using BackgroundRabbit.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundRabbit.Handlers;

public class ConsumerHandler
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _provider;

    public ConsumerHandler(
        ILoggerFactory loggerFactory,
        IServiceProvider provider
    )
    {
        _logger = loggerFactory.CreateLogger<ConsumerHandler>();
        _provider = provider;
    }

    public async Task HandleMessage(Message content, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ConsumerHandler received {content}", content);

        using var scope = _provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<MessagesContext>();

        context.Messages.Add(content);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Content saved to DB");
    }
}
