using Beef.Demo.Business;
using Beef.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.Functions.Subscribers
{
    [EventSubscriber("Demo.Robot.*", "PowerSourceChange")]
    public class PowerSourceChangeSubscriber : EventSubscriber<Guid>
    {
        private readonly IRobotManager _mgr;

        public PowerSourceChangeSubscriber(IRobotManager mgr)
        {
            _mgr = Check.NotNull(mgr, nameof(mgr));
            DataNotFoundHandling = ResultHandling.ContinueWithAudit;
            MaxAttempts = 5;
        }

        public override async Task<Result> ReceiveAsync(EventData<Guid> @event)
        {
            if (@event.Value == null)
                return Result.InvalidData($"Key '{@event.Key ?? "null"}' must be a GUID.", ResultHandling.ContinueWithAudit);

            if (@event.Value == new Guid(88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                throw new DivideByZeroException("The mystery 88 guid can't be divided by zero.");

            var id = @event.Value == new Guid(99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) ? new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) : @event.Value;

            var robot = await _mgr.GetAsync(id);
            if (robot == null)
                return Result.DataNotFound();

            robot.AcceptChanges();
            robot.PowerSource = id == new Guid(99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) ? "Q" : "N";
            if (robot.IsChanged)
                await _mgr.UpdateAsync(robot, @event.Value);

            Logger.LogInformation("A trace message to prove it works!");

            return Result.Success();
        }
    }
}