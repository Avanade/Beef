namespace Beef.Demo.Business
{
    public partial class RobotManager
    {
        private Task RaisePowerSourceChangeOnImplementationAsync(Guid id, RefDataNamespace.PowerSource powerSource)
            => _eventPublisher
                .PublishValueEvent(powerSource, $"Demo.Robot", "PowerSourceChange", id)
                .SendAsync();
    }
}