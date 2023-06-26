namespace Beef.Demo.Business.Data
{
    public partial class ReferenceDataData
    {
        private static Task<Result<CompanyCollection>> CompanyGetAll_OnImplementationAsync()
        {
            CoreEx.ExecutionContext.GetRequiredService<ILogger<ReferenceDataData>>().LogInformation("Getting company data....");

            return Task.FromResult(Result.Ok(new CompanyCollection
            {
                new Company { Id = Guid.NewGuid(), Code = "XYZ", Text = "XYZ", IsActive = true, SortOrder = 1, ExternalCode = "usmf" }
            }));
        }

        private static Task<Result<RefDataNamespace.CommunicationTypeCollection>> CommunicationTypeGetAll_OnImplementationAsync() => Task.FromResult(Result.Ok(new CommunicationTypeCollection
        {
            new CommunicationType { Id = 1, Code = "HOME", Text = "Home phone" },
            new CommunicationType { Id = 2, Code = "FAX", Text = "Fax number" },
            new CommunicationType { Id = 3, Code = "MOBILE", Text = "Mobile phone" },
            new CommunicationType { Id = 4, Code = "WORK", Text = "Work phone" }
        }));
    }
}