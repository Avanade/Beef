using Beef.Events.EventHubs;
using AzureEventHubs = Azure.Messaging.EventHubs.Producer;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.EventSend
{
    class Program
    {
        static async Task Main()
        {
            var cs = Environment.GetEnvironmentVariable("Beef_EventHubConnectionString");
            var ehc = new AzureEventHubs.EventHubProducerClient(cs);
            var ehp = new EventHubProducer(ehc);

            Console.WriteLine("Options are:");
            Console.WriteLine(" x - Stop.");
            Console.WriteLine(" 1 - Non-subscribed Event.");
            Console.WriteLine(" 2 - Null Identifier.");
            Console.WriteLine(" 3 - Not Found Exception.");
            Console.WriteLine(" 4 - Unhandled Exception.");
            Console.WriteLine(" 5 - Invalid Data.");
            Console.WriteLine(" 6 - Success.");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter option: ");
                switch (Console.ReadLine())
                {
                    case "x":
                        return;

                    case "1":
                        ehp.Publish("Something.Random", "Blah");
                        await ehp.SendAsync();
                        break;

                    case "2":
                        ehp.Publish("Demo.Robot.123", "PowerSourceChange");
                        await ehp.SendAsync();
                        break;

                    case "3":
                        ehp.Publish("Demo.Robot.123", "PowerSourceChange", Guid.NewGuid());
                        await ehp.SendAsync();
                        break;

                    case "4":
                        ehp.Publish("Demo.Robot.88", "PowerSourceChange", new Guid(88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
                        await ehp.SendAsync();
                        break;

                    case "5":
                        ehp.PublishValue("Q", "Demo.Robot.1", "PowerSourceChange", new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
                        await ehp.SendAsync();
                        break;

                    case "6":
                        ehp.PublishValue("N", "Demo.Robot.1", "PowerSourceChange", new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
                        await ehp.SendAsync();
                        break;
                }

                ehp.Reset();
            }
        }
    }
}