using System;
using System.Threading.Tasks;

namespace Beef.Demo.Business.DataSvc
{
    public static partial class PersonDataSvc
    {
        static PersonDataSvc()
        {
            _markOnAfterAsync = MarkOnAfterAsync;
        }

        private static async Task MarkOnAfterAsync()
        {
            await Beef.Events.Event.PublishValueEventAsync("Wahlberg", "Demo.Mark", "Marked").ConfigureAwait(false);
        }

        private static Task<int> DataSvcCustomOnImplementationAsync()
        {
            throw new NotImplementedException();
        }
    }
}