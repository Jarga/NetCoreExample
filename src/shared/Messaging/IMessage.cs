using System;

namespace Shared.Messaging
{
    public interface IMessage
    {
        Guid Id { get; }
    }
}