namespace Beef.Demo.Business.DataSvc
{
    public partial class RobotDataSvc
    {
        private Task<Result<Robot?>> GetOnImplementationAsync(Guid id) => _data.GetAsync(id);
    }
}