namespace Beef.Demo.Business.DataSvc
{
    public partial class PersonDataSvc
    {
        partial void PersonDataSvcCtor()
        {
            _markOnAfterAsync = MarkOnAfterAsync;
            _updateWithRollbackOnAfterAsync = _ => throw new InvalidOperationException("Some made up exception to validate that the update rolled back as expected.");
        }

        private Task MarkOnAfterAsync()
        {
            _events.PublishValueEvent("Wahlberg", "Demo.Mark", "Marked");
            return Task.CompletedTask;
        }

        private Task<int> DataSvcCustomOnImplementationAsync()
        {
            throw new NotImplementedException();
        }

        private Task<Person> EventPublishNoSendOnImplementationAsync(Person value)
        {
            _events.PublishValueEvent(value, $"Beef.Demo.NoSend.{value.Id}");
            return Task.FromResult(value);
        }

        private static Task ParamCollOnImplementationAsync(AddressCollection addresses)
        {
            return Task.CompletedTask;
        }
    }
}