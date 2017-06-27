using System;

namespace worker
{
    class Program
    {
        public static string AppUrl = "";
        public static string AMQPUrl = "";
        public static string Queue = "";

        static void Main(string[] args)
        {
            var bus = new MessageBus(AMQPUrl);

            bus.Subscribe<CreateMessage>(Queue, model => { });
        }
    }
}
