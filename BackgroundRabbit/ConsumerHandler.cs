using Microsoft.Extensions.Logging;

namespace BackgroundRabbit
{
    public class ConsumerHandler
    {
        private readonly ILogger _logger;

        public ConsumerHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConsumerHandler>();
        }

        public void HandleMessage(Message content)
        {
            _logger.LogInformation("ConsumerHandler [x] received {0}", content);
        }
    }
}
