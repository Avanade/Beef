using System;
using System.Threading.Tasks;

namespace Beef.Demo.Business.DataSvc
{
    public partial class PersonDataSvc
    {
        partial void PersonDataSvcCtor()
        {
            _markOnAfterAsync = MarkOnAfterAsync;
        }

        private async Task MarkOnAfterAsync()
        {
            await _evtPub.PublishValueEventAsync("Wahlberg", "Demo.Mark", "Marked").ConfigureAwait(false);
        }

        private Task<int> DataSvcCustomOnImplementationAsync()
        {
            throw new NotImplementedException();
        }
    }
}