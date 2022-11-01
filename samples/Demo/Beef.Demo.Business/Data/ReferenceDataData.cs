namespace Beef.Demo.Business.Data
{
    public partial class ReferenceDataData
    {
        private Task<CompanyCollection> CompanyGetAll_OnImplementationAsync()
        {
            CoreEx.ExecutionContext.GetRequiredService<ILogger<ReferenceDataData>>().LogInformation("Getting company data....");

            return Task.FromResult(new CompanyCollection
            {
                new Company { Id = Guid.NewGuid(), Code = "XYZ", Text = "XYZ", IsActive = true, SortOrder = 1, ExternalCode = "usmf" }
            });
        }
    }
}