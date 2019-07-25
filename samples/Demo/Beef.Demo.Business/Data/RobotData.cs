using Beef.Data.Cosmos;
using Beef.Demo.Common.Entities;
using System.Linq;

namespace Beef.Demo.Business.Data
{
    public partial class RobotData
    {
        public RobotData()
        {
            _getByArgsOnQuery = GetByArgsOnQuery;
        }

        private IQueryable<Robot> GetByArgsOnQuery(IQueryable<Robot> q, RobotArgs args, ICosmosDbArgs dbArgs)
        {
            q = q.WhereWildcard(x => x.ModelNo, args?.ModelNo);
            q = q.WhereWildcard(x => x.SerialNo, args?.SerialNo);
            return q.OrderBy(x => x.SerialNo);
        }
    }
}