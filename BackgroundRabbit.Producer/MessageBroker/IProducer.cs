namespace BackgroundRabbit.Producer.MessageBroker;

public interface IProducer
{
    void PushMessage(
        string routingKey,
        object message
    );
}
