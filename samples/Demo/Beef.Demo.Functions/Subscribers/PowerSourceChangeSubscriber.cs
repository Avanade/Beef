﻿using Beef.Demo.Business;
using Beef.Events;
using Beef.Events.Subscribe;
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
                var robot = await _mgr.GetAsync(id);
                if (robot == null)
                    return Result.DataNotFound();

                robot.AcceptChanges();
                robot.PowerSource = @event.Value;
                if (robot.IsChanged)
                    await _mgr.UpdateAsync(robot, id);

                return Result.Success();
            }
            else
                return Result.InvalidData($"Key '{@event.Key}' must be a GUID.", ResultHandling.ContinueWithAudit);
        }
    }
}