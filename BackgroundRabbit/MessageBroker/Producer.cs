using System;
using System.Text;
using BackgroundRabbit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace BackgroundRabbit.MessageBroker
{
    public class Producer :IProducer
    {
        private readonly IModel _channel;
        private readonly ILogger _logger;

        public Producer(
            IConfiguration configuration,
            ILogger<Producer> logger
        )
        {
            try
            {
                var connectionStringConfiguration = configuration.GetSection("MessageBroker:Host");

                var factory = new ConnectionFactory()
                    {HostName = connectionStringConfiguration.Value};

                var connection = factory.CreateConnection();
                _channel = connection.CreateModel();
            }
            catch (Exception ex)
            {
                logger.LogError(-1, ex, "RabbitMQClient init fail");
            }

            _logger = logger;
        }

        public void PushMessage(
            string routingKey,
            object message
        )
        {
            _logger.LogInformation($"PushMessage,routingKey:{routingKey}");

            string jsonMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _channel.BasicPublish(
                exchange: MessageConstants.FirstExchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );
        }
    }
}
