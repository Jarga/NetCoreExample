using System;

namespace worker
{
    public class CreateMessage : IMessage
    {

        public string Id { get; }

        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}