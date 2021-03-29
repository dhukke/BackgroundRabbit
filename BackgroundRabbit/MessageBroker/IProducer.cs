namespace BackgroundRabbit.MessageBroker
{
    public interface IProducer
    {
        void PushMessage(
            string routingKey,
            object message
        );
    }
}
