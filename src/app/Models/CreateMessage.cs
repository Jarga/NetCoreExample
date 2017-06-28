using System;
using Shared.Messaging;

namespace NetCoreExample.Models
{
    public class CreateMessage : IMessage
    {
        public Guid Id { get; set; }
    }
}