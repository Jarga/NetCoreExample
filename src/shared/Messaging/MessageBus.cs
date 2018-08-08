using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.Messaging
{
    public class MessageBus : IDisposable
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly object locker = new object();
        private bool disposedValue;

        public MessageBus(string messageBusConnectionString)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(messageBusConnectionString),
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = 20
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ConfirmSelect();
        }

        public void Publish(IMessage message, string exchangeOrQueue, string key = null)
        {
            if(string.IsNullOrWhiteSpace(exchangeOrQueue)) return;
            
            var hasKey = !string.IsNullOrWhiteSpace(key);
            if (!hasKey) {
                channel.QueueDeclare(exchangeOrQueue, true, false, false);
                channel.ExchangeDeclare(exchangeOrQueue + "Exchange", "topic", true);
                channel.QueueBind(exchangeOrQueue, exchangeOrQueue + "Exchange", "default");
            } else {
                channel.ExchangeDeclare(exchangeOrQueue, "topic", true);
            }

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2; //persistent
            props.CorrelationId = message.Id.ToString();

            lock (locker)
            {
                channel.BasicPublish(exchangeOrQueue + (hasKey ? "" : "Exchange"), hasKey ? key : "default", props, body);
                channel.WaitForConfirmsOrDie();
            }
        }

        public void Subscribe<T>(string queue, Action<T> callback)
        {
            if (!string.IsNullOrWhiteSpace(queue))
            {
                channel.QueueDeclare(queue, true, false, false);
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var payload = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(ea.Body));
                callback(payload);
            };

            lock (locker)
            {
                channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;
            lock (locker)
            {
                if (disposing)
                {
                    if (channel.IsOpen) channel.Close();
                    channel.Dispose();

                    if (connection.IsOpen) connection.Close();
                    connection.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
    }
}