using Beef.Entities;
using Beef.Events;
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
                        ehp.Publish("Demo.Robot.Null", "PowerSourceChange");
                        await ehp.SendAsync();
                        break;

                    case "3":
                        ehp.PublishValue(new PowerSourceChangeData { RobotId = Guid.NewGuid(), PowerSource = "N" }, "Demo.Robot.???", "PowerSourceChange");
                        await ehp.SendAsync();
                        break;

                    case "4":
                        ehp.PublishValue(new PowerSourceChangeData { RobotId = new Guid(88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), PowerSource = "N" }, "Demo.Robot.88", "PowerSourceChange");
                        await ehp.SendAsync();
                        break;

                    case "5":
                        ehp.PublishValue(new PowerSourceChangeData { RobotId = new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), PowerSource = "!" }, "Demo.Robot.2", "PowerSourceChange");
                        await ehp.SendAsync();
                        break;

                    case "6":
                        var ed = EventData.CreateValueEvent(new PowerSourceChangeData { RobotId = new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), PowerSource = "N" }, "Demo.Robot.2", "PowerSourceChange");
                        ed.Key = new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        ed.PartitionKey = PartitionKeyGenerator.Generate(Guid.NewGuid());
                        ehp.Publish(ed);
                        await ehp.SendAsync();
                        break;

                    case "11":
                        sbs.Publish("Something.Random", "Blah");
                        await sbs.SendAsync();
                        break;

                    case "12":
                        sbs.Publish("Demo.Robot.Null", "PowerSourceChange");
                        await sbs.SendAsync();
                        break;

                    case "13":
                        sbs.PublishValue(new PowerSourceChangeData { RobotId = Guid.NewGuid(), PowerSource = "N" }, "Demo.Robot.???", "PowerSourceChange");
                        await sbs.SendAsync();
                        break;

                    case "14":
                        sbs.PublishValue(new PowerSourceChangeData { RobotId = new Guid(88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), PowerSource = "N" }, "Demo.Robot.88", "PowerSourceChange");
                        await sbs.SendAsync();
                        break;

                    case "15":
                        sbs.PublishValue(new PowerSourceChangeData { RobotId = new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), PowerSource = "!" }, "Demo.Robot.2", "PowerSourceChange");
                        await sbs.SendAsync();
                        break;

                    case "16":
                        ed = EventData.CreateValueEvent(new PowerSourceChangeData { RobotId = new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), PowerSource = "N" }, "Demo.Robot.2", "PowerSourceChange");
                        ed.Key = new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
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

    public class PowerSourceChangeData
    {
        public Guid RobotId { get; set; }

        public string PowerSource { get; set; }
    }
}