using BackgroundRabbit.Producer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace BackgroundRabbit.Producer.MessageBroker;

public class ProducerChannelWaste : IProducer
{
    private readonly IConnection? _connection;
    private readonly ILogger _logger;

    public ProducerChannelWaste(
        IConfiguration configuration,
        ILogger<Producer> logger
    )
    {
        _logger = logger;

        try
        {
            var connectionStringConfiguration = configuration.GetSection("MessageBroker:Host");

            var factory = new ConnectionFactory()
            {
                HostName = connectionStringConfiguration.Value
            };

            _connection = factory.CreateConnection();
        }
        catch (Exception ex)
        {
            logger.LogError(-1, ex, "RabbitMQClient init fail");
        }
    }

    public void PushMessage(
        string routingKey,
        object message
    )
    {
        if (_connection is null)
        {
            _logger.LogWarning("Connection is null, not possible to pushMessage to routingKey: {routingKey}", routingKey);
            return;
        }

        var channel = _connection.CreateModel();

        try
        {
            _logger.LogInformation("Channel number: {ChannelNumber}", channel.ChannelNumber);

            _logger.LogInformation("PushMessage to routingKey: {routingKey}", routingKey);

            string jsonMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            if (channel is null)
            {
                _logger.LogWarning("Channel is null, not possible to pushMessage to routingKey: {routingKey}", routingKey);
                return;
            }

            channel.BasicPublish(
                exchange: MessageConstants.FirstExchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );

        }

        finally
        {
            // https://www.rabbitmq.com/dotnet-api-guide.html#connection-and-channel-lifespan
            // this will close the channel, but as it is better to have a long-live this is not the best solution:
            //channel.Close(); 
        }
    }
}
