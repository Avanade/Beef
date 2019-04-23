using Beef.Demo.Common.Entities;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.Business.Data
{
    public partial class ReferenceDataData
    {
        private async Task CompanyGetAll_OnImplementation(CompanyCollection coll)
        {
            coll.Add(new Company { Id = Guid.NewGuid(), Code = "XYZ", Text = "XYZ", IsActive = true, SortOrder = 1, ExternalCode = "usmf" });
            await Task.Delay(0);
        }
    }
}
