using Beef.Demo.Business;
using Beef.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.Functions.Subscribers
{
    [EventSubscriber("Demo.Robot.*", "PowerSourceChange")]
    public class PowerSourceChangeSubscriber : EventSubscriber<PowerSourceChangeData>
    {
        private readonly IRobotManager _mgr;

        public PowerSourceChangeSubscriber(IRobotManager mgr)
        {
            _mgr = Check.NotNull(mgr, nameof(mgr));
            DataNotFoundHandling = ResultHandling.ContinueWithAudit;
            MaxAttempts = 5;
        }

        public override async Task<Result> ReceiveAsync(EventData<PowerSourceChangeData> @event)
        {
            var data = @event.Value;
            if (data.RobotId == new Guid(88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                throw new DivideByZeroException("The mystery 88 guid can't be divided by zero.");

            var robot = await _mgr.GetAsync(data.RobotId);
            if (robot == null)
                return Result.DataNotFound();

            robot.AcceptChanges();
            robot.PowerSource = data.PowerSource;
            if (robot.IsChanged)
                await _mgr.UpdateAsync(robot, data.RobotId);

            Logger.LogInformation("A trace message to prove it works!");

            if (GetOriginatingData() == null)
                Logger.LogError("There should always be originating data!");

            return Result.Success();
        }
    }

    public class PowerSourceChangeData
    {
        public Guid RobotId { get; set; }

        public string PowerSource { get; set; }
    }
}