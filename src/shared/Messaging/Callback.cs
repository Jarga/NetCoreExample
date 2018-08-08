using RabbitMQ.Client;
using System;

namespace Shared.Messaging
{
    public class Callback
    {
        public Callback(Type serializerType, Action<object, IBasicProperties> handler, string handlerKey = null)
        {
            SerializerType = serializerType ?? throw new ArgumentNullException(nameof(serializerType));
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            HandlerKey = handlerKey;
        }

        public Type SerializerType { get; }

        public string HandlerKey { get; }

        public Action<object, IBasicProperties> Handler { get; }
    }
}