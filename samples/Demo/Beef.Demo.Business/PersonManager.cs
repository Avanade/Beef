using Beef.Demo.Common.Entities;
using System;
using System.Threading.Tasks;

#nullable enable

namespace Beef.Demo.Business
{
    public partial class PersonManager
    {
        partial void PersonManagerCtor()
        {
            _updateOnPreValidateAsync = UpdateOnPreValidateAsync;
        }

        private Task AddOnImplementationAsync(Person value)
        {
            Check.NotNull(value, nameof(value));
            return Task.CompletedTask;
        }

        private Task<Person?> ManagerCustomOnImplementationAsync()
        {
            return Task.FromResult<Person?>(null);
        }

        private async Task UpdateOnPreValidateAsync(Person value, Guid id)
        {
            var curr = await GetAsync(id).ConfigureAwait(false);
            if (ReferenceEquals(value, curr))
                throw new InvalidOperationException("The Get and Update person cannot should not have the same reference!");
        }
    }
}

#nullable restore