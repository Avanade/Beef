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
            _events.CreateValueEventAndPublish("Wahlberg", "Demo.Mark", "Marked");
            return Task.CompletedTask;
        }

        private Task<int> DataSvcCustomOnImplementationAsync()
        {
            throw new NotImplementedException();
        }

        private Task<Person> EventPublishNoSendOnImplementationAsync(Person value)
        {
            _events.CreateValueEventAndPublish(value, $"Beef.Demo.NoSend.{value.Id}");
            return Task.FromResult(value);
        }

        private Task ParamCollOnImplementationAsync(AddressCollection addresses)
        {
            return Task.CompletedTask;
        }
    }
}