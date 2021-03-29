using System;
using System.Threading.Tasks;
using BackgroundRabbit.Entities;
using BackgroundRabbit.Handlers;
using BackgroundRabbit.MessageBroker;
using BackgroundRabbit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackgroundRabbit.Workers
{
    public class ConsumerWorker : Consumer
    {
        private readonly IServiceProvider _provider;

        public ConsumerWorker(
            IConfiguration configuration,
            ILogger<ConsumerWorker> logger,
            IServiceProvider provider
        ) : base(configuration, logger)
        {
            _provider = provider;

            RouteKey = MessageConstants.FirstRountingKey;
            QueueName = MessageConstants.FirstQueue;
        }

        protected override async Task<bool> Process(string message)
        {
            var messageMessage = JsonConvert.DeserializeObject<Message>(message);

            try
            {
                using var scope = _provider.CreateScope();
                var consumerHandler = scope.ServiceProvider.GetRequiredService<ConsumerHandler>();
                consumerHandler.HandleMessage(messageMessage);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Fail processing Message: ", message);
                Logger.LogError(-1, e, "Process Fail");

                return false;
            }
        }
    }
}
