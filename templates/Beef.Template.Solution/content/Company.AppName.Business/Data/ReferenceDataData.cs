namespace Company.AppName.Business.Data;

public partial class ReferenceDataData
{
    private Task<GenderCollection> GenderGetAll_OnImplementationAsync()
    {
        return Task.FromResult(new GenderCollection
        {
            new Gender { Id = Guid.NewGuid(), Code = "F", Text = "Female", IsActive = true, SortOrder = 1 },
            new Gender { Id = Guid.NewGuid(), Code = "M", Text = "Male", IsActive = true, SortOrder = 2 }
        });
    }
}