using System;

namespace BackgroundRabbit.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Content)}: {Content}, {nameof(DateTime)}: {DateTime}";
        }
    }
}
