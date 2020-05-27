using Beef.Demo.Business;
using Beef.Events;
using Beef.Events.Subscribe;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.Functions.Subscribers
{
    public class PowerSourceChangeSubscriber : EventSubscriber<string>
    {
        public PowerSourceChangeSubscriber() : base("Demo.Robot.*", "PowerSourceChange") 
        {
            DataNotFoundHandling = ResultHandling.ContinueWithAudit;
        }

        public override async Task<Result> ReceiveAsync(EventData<string> @event)
        {
            var mgr = Factory.Create<IRobotManager>();
            if (@event.Key is Guid id)
            {
                var robot = await mgr.GetAsync(id);
                if (robot == null)
                    return Result.DataNotFound();

                robot.AcceptChanges();
                robot.PowerSource = @event.Value;
                if (robot.IsChanged)
                    await mgr.UpdateAsync(robot, id);

                return Result.Success();
            }
            else
                return Result.InvalidData($"Key '{@event.Key}' must be a GUID.", ResultHandling.ContinueWithAudit);
        }
    }
}