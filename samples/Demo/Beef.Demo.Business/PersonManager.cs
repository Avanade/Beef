using Beef.Demo.Common.Entities;
using System.Threading.Tasks;

#nullable enable

namespace Beef.Demo.Business
{
    public partial class PersonManager
    {
        private Task AddOnImplementationAsync(Person value)
        {
            Check.NotNull(value, nameof(value));
            return Task.CompletedTask;
        }

        private Task<Person?> ManagerCustomOnImplementationAsync()
        {
            return Task.FromResult<Person?>(null);
        }
    }
}

#nullable restore