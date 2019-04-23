using Beef.Data.OData;
using Beef.Demo.Common.Entities;
using System.Linq;

namespace Beef.Demo.Business.Data
{
    public partial class ProductData
    {
        public ProductData()
        {
            _getByArgsOnQuery = GetByArgs_OnQuery;
        }

        private IQueryable<Product> GetByArgs_OnQuery(IQueryable<Product> q, ProductArgs args, IODataArgs da)
        {
            return q.WhereWildcard(p => p.Name, args?.Name)
                .WhereWildcard(p => p.Description, args?.Description);
        }
    }
}