using Beef.Data.OData;
using Beef.Demo.Common.Entities;
using Beef.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Demo.Business.Data
{
    public partial class CustomerGroupData
    {
        public CustomerGroupData()
        {
            _getByArgsOnQuery = GetByArgsOnQuery;
        }

        private IQueryable<CustomerGroup> GetByArgsOnQuery(IQueryable<CustomerGroup> q, CustomerGroupArgs args, IODataArgs odataArgs)
        {
            return q.WhereWith(args?.Company, a => a.Company == args.Company)
                    .WhereWith(args?.Description, a => a.Description == args.Description);
        }

        private async Task UpdateBatchOnImplementationAsync(CustomerGroupCollection values)
        {
            var batch = DynamicsAx.Default.CreateBatch();
            foreach (var v in values)
            {
                await batch.UpdateAsync(ODataMapper.Default.CreateArgs(ODataIfMatch.Upsert), v).ConfigureAwait(false);
            }

            var resp = await batch.SendAsync().ConfigureAwait(false);
            resp.EnsureBatchSuccessStatusCode();
        }
    }
}
