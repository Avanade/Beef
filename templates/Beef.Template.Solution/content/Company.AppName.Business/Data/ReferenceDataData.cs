using System;
using System.Threading.Tasks;
using Company.AppName.Business.Entities;

namespace Company.AppName.Business.Data
{
    public partial class ReferenceDataData
    {
        private Task GenderGetAll_OnImplementation(GenderCollection coll)
        {
            coll.Add(new Gender { Id = Guid.NewGuid(), Code = "F", Text = "Female", IsActive = true, SortOrder = 1 });
            coll.Add(new Gender { Id = Guid.NewGuid(), Code = "M", Text = "Male", IsActive = true, SortOrder = 2 });
            return Task.CompletedTask;
        }
    }
}