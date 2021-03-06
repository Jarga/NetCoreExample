using System;
using Shared.Messaging;

namespace Worker
{
    public class CreateMessage : IMessage
    {

        public Guid Id { get; set; }

        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}