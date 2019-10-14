using Beef.Demo.Business;
using Beef.Events;
using Beef.Events.Subscribe;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.Functions.Subscribers
{
    public class PowerSourceChangeSubscriber : EventSubscriber<string>
    {
        public PowerSourceChangeSubscriber() : base("Demo.Robot.*", "PowerSourceChange") { }

        public override async Task ReceiveAsync(EventData<string> @event)
        {
            var mgr = Factory.Create<IRobotManager>();
            if (Guid.TryParse((string)@event.Key, out var id))
            {
                var robot = await mgr.GetAsync(id);
                if (robot == null)
                    return;

                robot.AcceptChanges();
                robot.PowerSource = @event.Value;
                if (robot.IsChanged)
                    await mgr.UpdateAsync(robot, id);
            }
        }
    }
}