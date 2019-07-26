using System;
using System.Threading.Tasks;
using MassTransit;

namespace MassTransitConsole
{
    public class YourMessage
    {
        public string Text { get; set; }
    }

    public class Program
    {
        public static async Task Main()
        {
            var bus = Bus.Factory.CreateUsingInMemory(sbc =>
            {
                sbc.ReceiveEndpoint("test_queue", ep =>
                {
                    ep.Handler<YourMessage>(context =>
                    {
                        return Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
                    });
                });
            });

            await bus.StartAsync();

            await bus.Publish(new YourMessage { Text = "Hi" });

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            bus.Stop();
        }
    }
}
