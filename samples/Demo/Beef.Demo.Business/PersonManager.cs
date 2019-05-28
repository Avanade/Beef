using Beef.Demo.Common.Entities;
using System.Threading.Tasks;

namespace Beef.Demo.Business
{
    public partial class PersonManager
    {
        private Task AddOnImplementationAsync(Person value)
        {
            Check.NotNull(value, nameof(value));
            return Task.CompletedTask;
        }
    }
}