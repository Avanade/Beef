using Beef.Events;
using System;
using System.Threading.Tasks;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business
{
    public partial class RobotManager
    {
        private async Task RaisePowerSourceChangeOnImplementationAsync(Guid id, RefDataNamespace.PowerSource powerSource)
        {
            var e = new EventData<string>
            {
                Subject = $"Demo.Robot.{id}",
                Action = "PowerSourceChange",
                Value = powerSource,
                Key = id
            };

            await _eventPublisher.Publish(e).SendAsync().ConfigureAwait(false);
        }
    }
}