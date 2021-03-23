using System;

namespace BackgroundRabbit
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Content)}: {Content}";
        }
    }
}
