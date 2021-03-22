using System;

namespace BackgroundRabbit
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Text)}: {Text}";
        }
    }
}
