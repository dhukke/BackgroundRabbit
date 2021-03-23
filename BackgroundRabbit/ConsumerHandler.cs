using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackgroundRabbit
{
    public class ConsumerHandler
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _provider;

        public ConsumerHandler(ILoggerFactory loggerFactory, IServiceProvider provider)
        {
            _logger = loggerFactory.CreateLogger<ConsumerHandler>();
            _provider = provider;
        }

        public void HandleMessage(Message content)
        {
            _logger.LogInformation("ConsumerHandler [x] received {0}", content);


            using (var scope = _provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MessagesContext>();

                context.Messages.Add(content);

                context.SaveChanges();

                var dbContent = context.Messages.FirstOrDefault(x => x.Id == content.Id);

                _logger.LogInformation("ConsumerHandler [x] read from DB: {Id}, {Content}", dbContent.Id, dbContent.Content);
            }
        }
    }
}
