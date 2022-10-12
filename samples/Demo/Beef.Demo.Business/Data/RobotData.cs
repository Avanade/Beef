namespace Beef.Demo.Business.Data
{
    public partial class RobotData
    {
        partial void RobotDataCtor()
        {
            _getByArgsOnQuery = GetByArgsOnQuery;
        }

        private IQueryable<Model.Robot> GetByArgsOnQuery(IQueryable<Model.Robot> q, RobotArgs? args)
        {
            q = q.WhereWildcard(x => x.ModelNo, args?.ModelNo);
            q = q.WhereWildcard(x => x.SerialNo, args?.SerialNo);
            q = q.WhereWith(args?.PowerSources, x => args!.PowerSources!.ToCodeList().Contains(x.PowerSource));
            return q.OrderBy(x => x.SerialNo);
        }
    }
}