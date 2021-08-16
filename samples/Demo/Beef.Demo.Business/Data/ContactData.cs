using Beef.Demo.Business.Entities;
using Beef.Events;
using Beef.Mapper;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.Business.Data
{
    public partial class ContactData
    {
        private async Task RaiseEventOnImplementationAsync(bool throwError)
        {
            // Nesting invoker should result in the event being enqueued into the database (not committed though).
            await _ef.EventOutboxInvoker.InvokeAsync(this, () => 
            { 
                _evtPub.Publish("Contact", "Made");
                return Task.CompletedTask;
            });

            // The exception should result in a rollback of the database event enqueue.
            if (throwError)
                throw new DivideByZeroException();
        }
    }
}