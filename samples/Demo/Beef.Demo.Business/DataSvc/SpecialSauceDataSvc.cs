namespace Beef.Demo.Business.DataSvc
{
    public partial class SpecialSauceDataSvc(ISpecialSauceData data)
    {
        private readonly ISpecialSauceData _data = data.ThrowIfNull();

        public Task PourOnImplementationAsync() => _data.PourAsync();
    }
}