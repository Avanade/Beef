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
            return q.WhereWhen(a => a.Company == args.Company, args?.Company != null)
                    .WhereWhen(a => a.Description == args.Description, args?.Description != null);
        }

        private async Task UpdateBatchOnImplementationAsync(CustomerGroupCollection values)
        {
            var batch = DynamicsAx.Default.CreateBatch();
            foreach (var v in values)
            {
                await batch.UpdateAsync(ODataMapper.Default.CreateArgs(ODataIfMatch.Upsert), v);
            }

            var resp = await batch.SendAsync();
            resp.EnsureBatchSuccessStatusCode();
        }
    }
}
