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
            await Beef.Events.Event.PublishValueEventAsync("Wahlberg", "Demo.Mark", "Marked").ConfigureAwait(false);
        }

        private Task<int> DataSvcCustomOnImplementationAsync()
        {
            throw new NotImplementedException();
        }
    }
}