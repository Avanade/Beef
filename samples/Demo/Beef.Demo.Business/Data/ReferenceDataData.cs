namespace Beef.Demo.Business.Data
{
    public partial class ReferenceDataData
    {
        private Task<CompanyCollection> CompanyGetAll_OnImplementationAsync()
        {
            return Task.FromResult(new CompanyCollection
            {
                new Company { Id = Guid.NewGuid(), Code = "XYZ", Text = "XYZ", IsActive = true, SortOrder = 1, ExternalCode = "usmf" }
            });
        }
    }
}