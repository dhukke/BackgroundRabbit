namespace BackgroundRabbit.Producer.Entities;

public class Message
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public DateTime DateTime { get; set; }

    public override string ToString() => $"{nameof(Id)}: {Id}, {nameof(Content)}: {Content}, {nameof(DateTime)}: {DateTime}";
}
