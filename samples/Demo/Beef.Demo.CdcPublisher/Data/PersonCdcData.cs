using Beef.Demo.Common.Entities;
using Beef.Events;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Demo.CdcPublisher.Data
{
    public partial class PersonCdcData
    {
        protected override Task<IEnumerable<EventData>> CreateEventsAsync(PersonCdcWrapperCollection coll, CancellationToken cancellationToken)
            => CreateEventsWithGetAsync<Person>(coll, w => _personManager.GetAsync(w.PersonId), cancellationToken);
    }
}