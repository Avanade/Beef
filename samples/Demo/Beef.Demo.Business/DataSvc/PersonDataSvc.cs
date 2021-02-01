using System;
using System.Threading.Tasks;

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
            _evtPub.PublishValue("Wahlberg", "Demo.Mark", "Marked");
            return Task.CompletedTask;
        }

        private Task<int> DataSvcCustomOnImplementationAsync()
        {
            throw new NotImplementedException();
        }
    }
}