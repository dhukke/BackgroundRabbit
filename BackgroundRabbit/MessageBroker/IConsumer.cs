namespace BackgroundRabbit.MessageBroker
{
    public interface IConsumer
    {
        bool Process(string message);
    }
}
