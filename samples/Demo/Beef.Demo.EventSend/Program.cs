using Beef.Events;
using Beef.Events.Publish;
using Azure = Microsoft.Azure.EventHubs;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.EventSend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cs = Environment.GetEnvironmentVariable("Beef_EventHubConnectionString");
            var ehc = Azure.EventHubClient.CreateFromConnectionString(cs);
            var ehp = new EventHubPublisher(ehc);

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
                }

                ehp.Reset();
            }
        }
    }
}