using Beef.Entities;
using Beef.Events.EventHubs;
using Beef.Events.ServiceBus;
using System;
using System.Threading.Tasks;
using AzureEventHubs = Azure.Messaging.EventHubs.Producer;
using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace Beef.Demo.EventSend
{
    class Program
    {
        static async Task Main()
        {
            var cs = Environment.GetEnvironmentVariable("Beef_EventHubConnectionString");
            var ehc = new AzureEventHubs.EventHubProducerClient(cs);
            var ehp = new EventHubProducer(ehc);

            cs = Environment.GetEnvironmentVariable("Beef_ServiceBusConnectionString");
            var sbsc = new AzureServiceBus.ServiceBusClient(cs);
            var sbs = new ServiceBusSender(sbsc, "Default");

            Console.WriteLine("Options are:");
            Console.WriteLine(" x - Stop.");
            Console.WriteLine();
            Console.WriteLine("EventHubs...");
            Console.WriteLine(" 1 - Non-subscribed Event.");
            Console.WriteLine(" 2 - Null Identifier.");
            Console.WriteLine(" 3 - Not Found Exception.");
            Console.WriteLine(" 4 - Unhandled Exception.");
            Console.WriteLine(" 5 - Invalid Data.");
            Console.WriteLine(" 6 - Success.");
            Console.WriteLine();
            Console.WriteLine("ServiceBus...");
            Console.WriteLine(" 11 - Non-subscribed Event.");
            Console.WriteLine(" 12 - Null Identifier.");
            Console.WriteLine(" 13 - Not Found Exception.");
            Console.WriteLine(" 14 - Unhandled Exception.");
            Console.WriteLine(" 15 - Invalid Data.");
            Console.WriteLine(" 16 - Success.");
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
                        var ed = ehp.CreateValueEvent("N", "Demo.Robot.1", "PowerSourceChange");
                        ed.Key = new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        ed.PartitionKey = PartitionKeyGenerator.Generate(Guid.NewGuid());
                        ehp.Publish(ed);
                        await ehp.SendAsync();
                        break;

                    case "11":
                        sbs.Publish("Something.Random", "Blah");
                        await sbs.SendAsync();
                        break;

                    case "12":
                        sbs.Publish("Demo.Robot.123", "PowerSourceChange");
                        await sbs.SendAsync();
                        break;

                    case "13":
                        sbs.Publish("Demo.Robot.123", "PowerSourceChange", Guid.NewGuid());
                        await sbs.SendAsync();
                        break;

                    case "14":
                        sbs.Publish("Demo.Robot.88", "PowerSourceChange", new Guid(88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
                        await sbs.SendAsync();
                        break;

                    case "15":
                        sbs.PublishValue("Q", "Demo.Robot.1", "PowerSourceChange", new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
                        await sbs.SendAsync();
                        break;

                    case "16":
                        ed = sbs.CreateValueEvent("N", "Demo.Robot.1", "PowerSourceChange");
                        ed.Key = new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        ed.PartitionKey = PartitionKeyGenerator.Generate(Guid.NewGuid());
                        sbs.Publish(ed);
                        await sbs.SendAsync();
                        break;
                }

                ehp.Reset();
                sbs.Reset();
            }
        }
    }
}