﻿using Beef.Demo.Business;
using Beef.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.Functions.Subscribers
{
    [EventSubscriber("Demo.Robot.*", "PowerSourceChange")]
    public class PowerSourceChangeSubscriber : EventSubscriber<string>
    {
        private readonly IRobotManager _mgr;

        public PowerSourceChangeSubscriber(IRobotManager mgr)
        {
            _mgr = Check.NotNull(mgr, nameof(mgr));
            DataNotFoundHandling = ResultHandling.ContinueWithAudit;
        }

        public override async Task<Result> ReceiveAsync(EventData<string> @event)
        {
            if (@event.Key is Guid id)
            {
                if (id == new Guid(88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    throw new DivideByZeroException("The mystery 88 guid can't be divided by zero.");

                var robot = await _mgr.GetAsync(id);
                if (robot == null)
                    return Result.DataNotFound();

                robot.AcceptChanges();
                robot.PowerSource = @event.Value;
                if (robot.IsChanged)
                    await _mgr.UpdateAsync(robot, id);

                Logger.LogInformation("A trace message to prove it works!");

                return Result.Success();
            }
            else
                return Result.InvalidData($"Key '{@event.Key ?? "null"}' must be a GUID.", ResultHandling.ContinueWithAudit);
        }
    }
}