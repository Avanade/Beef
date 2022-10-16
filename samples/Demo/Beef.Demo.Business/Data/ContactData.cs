namespace Beef.Demo.Business.Data
{
    public partial class ContactData
    {
        private async Task RaiseEventOnImplementationAsync(bool throwError)
        {
            // Nesting invoker should result in the event being enqueued into the database (not committed though).
            //await _ef.EventOutboxInvoker.InvokeAsync(this, () => 
            //{ 
            //    _evtPub.Publish("Contact", "Made");
            //    return Task.CompletedTask;
            //});
            await _events.PublishEvent("Contact", "Made").SendAsync();

            // The exception should result in a rollback of the database event enqueue - fingers crossed ;-)
            if (throwError)
                throw new DivideByZeroException();
        }
    }
}