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

        private async Task MarkOnAfterAsync()
        {
            await _evtPub.PublishValueAsync("Wahlberg", "Demo.Mark", "Marked").ConfigureAwait(false);
        }

        private Task<int> DataSvcCustomOnImplementationAsync()
        {
            throw new NotImplementedException();
        }
    }
}